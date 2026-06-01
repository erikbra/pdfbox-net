#!/usr/bin/env node
/**
 * Downloads the Ghent PDF Output Suite V50 by automating the license-acceptance
 * form on the GWG download page with Playwright.
 *
 * Prerequisites:
 *   npm install playwright
 *   npx playwright install chromium
 *
 * Usage:
 *   node scripts/download-ghent-pdf.mjs [output-dir]
 *
 * The default output directory is "target/pdfs" relative to the repository root.
 * After extraction the following file must exist:
 *   <output-dir>/Ghent_PDF_Output_Suite_V50_Full/Categories/1-CMYK/Test pages/Ghent_PDF-Output-Test-V50_CMYK_X4.pdf
 */

import { chromium } from 'playwright';
import { createWriteStream, existsSync, mkdirSync } from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';
import { exec } from 'node:child_process';
import { promisify } from 'node:util';
import { get as httpsGet } from 'node:https';
import { get as httpGet } from 'node:http';

const execAsync = promisify(exec);

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const repoRoot = path.resolve(__dirname, '..');
const outputDir = process.argv[2] ?? path.join(repoRoot, 'target', 'pdfs');

const GWG_DOWNLOAD_URL = 'https://gwg.org/download/ghentpdfoutputsuitev50/';
const EXPECTED_FILE = path.join(
  outputDir,
  'Ghent_PDF_Output_Suite_V50_Full',
  'Categories',
  '1-CMYK',
  'Test pages',
  'Ghent_PDF-Output-Test-V50_CMYK_X4.pdf'
);

mkdirSync(outputDir, { recursive: true });

if (existsSync(EXPECTED_FILE)) {
  console.log('Ghent PDF already present, skipping download.');
  console.log(`  ${EXPECTED_FILE}`);
  process.exit(0);
}

console.log('Launching browser to download Ghent PDF Output Suite V50...');

const browser = await chromium.launch({ headless: true });
const context = await browser.newContext({ acceptDownloads: true });
const page = await context.newPage();

try {
  await page.goto(GWG_DOWNLOAD_URL, { waitUntil: 'domcontentloaded', timeout: 30_000 });

  // ---------------------------------------------------------------------------
  // Accept the license / terms-of-use form.
  // The GWG download page presents a form with a checkbox and a submit button.
  // Try several common selector patterns in priority order.
  // ---------------------------------------------------------------------------

  // 1. Accept checkbox (various ids / names used by the page over time)
  const checkboxSelectors = [
    'input[type="checkbox"][name*="accept"]',
    'input[type="checkbox"][id*="accept"]',
    'input[type="checkbox"][name*="agree"]',
    'input[type="checkbox"][id*="agree"]',
    'input[type="checkbox"][name*="terms"]',
    'input[type="checkbox"]',
  ];

  let checked = false;
  for (const sel of checkboxSelectors) {
    const el = page.locator(sel).first();
    if (await el.count() > 0) {
      await el.check();
      console.log(`  Checked license checkbox: ${sel}`);
      checked = true;
      break;
    }
  }
  if (!checked) {
    console.warn('  Warning: could not find a license checkbox — the page may have changed.');
  }

  // 2. Click the download / submit button and capture the resulting download
  const downloadButtonSelectors = [
    'input[type="submit"]',
    'button[type="submit"]',
    'a[href*="ghent"]',
    'a[href*="Ghent"]',
    'a[href*=".zip"]',
    'button:has-text("Download")',
    'a:has-text("Download")',
  ];

  /** @type {string | null} */
  let zipPath = null;

  for (const sel of downloadButtonSelectors) {
    const el = page.locator(sel).first();
    if (await el.count() === 0) continue;

    const href = await el.getAttribute('href').catch(() => null);

    if (href && href.includes('.zip')) {
      // Direct link — download without triggering browser download dialog
      const resolvedUrl = href.startsWith('http') ? href : new URL(href, GWG_DOWNLOAD_URL).toString();
      console.log(`  Downloading ZIP from direct link: ${resolvedUrl}`);
      zipPath = path.join(outputDir, 'GhentV50.zip');
      await downloadFile(resolvedUrl, zipPath);
      break;
    }

    // Otherwise submit/click and wait for a browser-initiated download
    console.log(`  Clicking download element: ${sel}`);
    const [download] = await Promise.all([
      page.waitForEvent('download', { timeout: 60_000 }),
      el.click(),
    ]);
    zipPath = path.join(outputDir, 'GhentV50.zip');
    await download.saveAs(zipPath);
    console.log(`  Saved download to: ${zipPath}`);
    break;
  }

  if (!zipPath || !existsSync(zipPath)) {
    throw new Error('Could not locate or download the Ghent ZIP file. The page structure may have changed.');
  }

  // ---------------------------------------------------------------------------
  // Extract the ZIP
  // ---------------------------------------------------------------------------
  console.log(`Extracting ${zipPath} into ${outputDir}...`);
  if (process.platform === 'win32') {
    await execAsync(`powershell -Command "Expand-Archive -Force -Path '${zipPath}' -DestinationPath '${outputDir}'"`);
  } else {
    await execAsync(`unzip -o "${zipPath}" -d "${outputDir}"`);
  }

  if (!existsSync(EXPECTED_FILE)) {
    throw new Error(
      `Extraction completed but expected file not found:\n  ${EXPECTED_FILE}\n` +
      `Check the ZIP contents in ${outputDir} and adjust the path in Rendering.cs if needed.`
    );
  }

  console.log('Ghent PDF Output Suite V50 ready:');
  console.log(`  ${EXPECTED_FILE}`);
} finally {
  await browser.close();
}

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

/**
 * Downloads a file from a URL to a local path, following redirects.
 * @param {string} url
 * @param {string} dest
 * @returns {Promise<void>}
 */
function downloadFile(url, dest) {
  return new Promise((resolve, reject) => {
    const file = createWriteStream(dest);
    const getter = url.startsWith('https') ? httpsGet : httpGet;
    getter(url, (res) => {
      if (res.statusCode >= 300 && res.statusCode < 400 && res.headers.location) {
        file.close();
        downloadFile(res.headers.location, dest).then(resolve).catch(reject);
        return;
      }
      if (res.statusCode !== 200) {
        file.close();
        reject(new Error(`HTTP ${res.statusCode} for ${url}`));
        return;
      }
      res.pipe(file);
      file.on('finish', () => file.close(resolve));
      file.on('error', reject);
    }).on('error', reject);
  });
}
