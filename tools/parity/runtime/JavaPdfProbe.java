import java.awt.image.BufferedImage;
import java.io.File;
import java.io.FileInputStream;
import java.io.InputStream;
import java.nio.charset.StandardCharsets;
import java.security.MessageDigest;
import java.util.ArrayList;
import java.util.HexFormat;
import java.util.List;
import javax.imageio.ImageIO;
import org.apache.pdfbox.cos.COSName;
import org.apache.pdfbox.Loader;
import org.apache.pdfbox.multipdf.PDFMergerUtility;
import org.apache.pdfbox.pdmodel.PDDocument;
import org.apache.pdfbox.pdmodel.PDDocumentCatalog;
import org.apache.pdfbox.pdmodel.PDDocumentInformation;
import org.apache.pdfbox.pdmodel.PDPage;
import org.apache.pdfbox.pdmodel.PDResources;
import org.apache.pdfbox.pdmodel.interactive.form.PDAcroForm;
import org.apache.pdfbox.pdmodel.interactive.form.PDField;
import org.apache.pdfbox.rendering.PDFRenderer;
import org.apache.pdfbox.text.PDFTextStripper;

public final class JavaPdfProbe {
    public static void main(String[] args) throws Exception {
        if (args.length < 2) {
            System.err.println("usage: JavaPdfProbe <out-dir> <pdf> [<pdf>...] | --merge <out-dir> <pdf-a> <pdf-b> | --structure <pdf> [<pdf>...]");
            System.exit(2);
        }
        if ("--merge".equals(args[0])) {
            if (args.length != 4) {
                System.err.println("usage: JavaPdfProbe --merge <out-dir> <pdf-a> <pdf-b>");
                System.exit(2);
            }
            merge(new File(args[1]), new File(args[2]), new File(args[3]));
            return;
        }
        if ("--structure".equals(args[0])) {
            for (int i = 1; i < args.length; i++) {
                structure(new File(args[i]));
            }
            return;
        }
        File outDir = new File(args[0]);
        outDir.mkdirs();
        for (int i = 1; i < args.length; i++) {
            probe(outDir, new File(args[i]));
        }
    }

    private static void probe(File outDir, File input) {
        String name = input.getName();
        int pages = -1;
        long started = System.nanoTime();
        try (PDDocument document = Loader.loadPDF(input)) {
            pages = document.getNumberOfPages();
            emit(name, "load", true, pages, "", elapsed(started));

            started = System.nanoTime();
            try {
                String text = new PDFTextStripper().getText(document);
                java.nio.file.Files.writeString(new File(outDir, stripExt(name) + "-java-text.txt").toPath(), text, StandardCharsets.UTF_8);
                emit(name, "text", true, pages, hash(text), elapsed(started));
            } catch (Throwable t) {
                emit(name, "text", false, pages, message(t), elapsed(started));
            }

            started = System.nanoTime();
            try {
                File saved = new File(outDir, stripExt(name) + "-java-copy.pdf");
                document.save(saved);
                emit(name, "save", true, pages, fileSignature(saved), elapsed(started));
            } catch (Throwable t) {
                emit(name, "save", false, pages, message(t), elapsed(started));
            }

            started = System.nanoTime();
            try {
                if (pages > 0) {
                    BufferedImage image = new PDFRenderer(document).renderImageWithDPI(0, 36);
                    File png = new File(outDir, stripExt(name) + "-java-p1.png");
                    ImageIO.write(image, "png", png);
                    emit(name, "render", true, pages, image.getWidth() + "x" + image.getHeight() + ":" + imagePixelHash(image) + ":" + imageMetrics(image), elapsed(started));
                } else {
                    emit(name, "render", true, pages, "no-pages", elapsed(started));
                }
            } catch (Throwable t) {
                emit(name, "render", false, pages, message(t), elapsed(started));
            }
        } catch (Throwable t) {
            emit(name, "load", false, pages, message(t), elapsed(started));
        }
    }

    private static void structure(File input) {
        long started = System.nanoTime();
        int pages = -1;
        try (PDDocument document = Loader.loadPDF(input)) {
            pages = document.getNumberOfPages();
            emit(input.getAbsolutePath(), "structure", true, pages, structuralSignature(document), elapsed(started));
        } catch (Throwable t) {
            emit(input.getAbsolutePath(), "structure", false, pages, message(t), elapsed(started));
        }
    }

    static void merge(File outDir, File a, File b) {
        long started = System.nanoTime();
        try {
            PDFMergerUtility merger = new PDFMergerUtility();
            merger.addSource(a);
            merger.addSource(b);
            File dest = new File(outDir, stripExt(a.getName()) + "__" + stripExt(b.getName()) + "-java-merged.pdf");
            merger.setDestinationFileName(dest.getAbsolutePath());
            merger.mergeDocuments(null);
            emit(a.getName() + "+" + b.getName(), "merge", true, -1, fileSignature(dest), elapsed(started));
        } catch (Throwable t) {
            emit(a.getName() + "+" + b.getName(), "merge", false, -1, message(t), elapsed(started));
        }
    }

    private static String stripExt(String name) {
        int dot = name.lastIndexOf('.');
        return dot < 0 ? name : name.substring(0, dot);
    }

    private static long elapsed(long started) {
        return (System.nanoTime() - started) / 1_000_000L;
    }

    private static String hash(String text) throws Exception {
        byte[] bytes = text.getBytes(StandardCharsets.UTF_8);
        byte[] digest = MessageDigest.getInstance("SHA-256").digest(bytes);
        return bytes.length + ":" + HexFormat.of().formatHex(digest).substring(0, 16);
    }

    private static String fileSignature(File file) throws Exception {
        MessageDigest digest = MessageDigest.getInstance("SHA-256");
        try (InputStream input = new FileInputStream(file)) {
            byte[] buffer = new byte[8192];
            int read;
            while ((read = input.read(buffer)) >= 0) {
                digest.update(buffer, 0, read);
            }
        }
        return file.length() + ":" + HexFormat.of().formatHex(digest.digest()).substring(0, 16);
    }

    private static String structuralSignature(PDDocument document) throws Exception {
        List<String> parts = new ArrayList<>();
        int pages = document.getNumberOfPages();
        parts.add("pages=" + pages);
        parts.add("info=" + documentInformationSignature(document.getDocumentInformation()));
        parts.add("forms=" + formSignature(document.getDocumentCatalog()));
        parts.add("pageShape=" + pageShapeSignature(document));
        parts.add("text=" + hash(new PDFTextStripper().getText(document)));
        if (pages > 0) {
            BufferedImage image = new PDFRenderer(document).renderImageWithDPI(0, 36);
            parts.add("render=" + image.getWidth() + "x" + image.getHeight() + ":" + imagePixelHash(image) + ":" + imageMetrics(image));
        } else {
            parts.add("render=no-pages");
        }
        return String.join("|", parts);
    }

    private static String documentInformationSignature(PDDocumentInformation info) {
        List<String> entries = new ArrayList<>();
        for (String key : info.getMetadataKeys()) {
            String value = info.getCustomMetadataValue(key);
            entries.add(key + "=" + (value == null ? "" : value));
        }
        return String.join(",", entries);
    }

    private static String formSignature(PDDocumentCatalog catalog) {
        PDAcroForm form = catalog.getAcroForm(null);
        if (form == null) {
            return "fields=0";
        }

        List<String> fields = new ArrayList<>();
        for (PDField field : form.getFieldTree()) {
            fields.add(field.getFullyQualifiedName());
        }
        return "fields=" + fields.size() + ":" + String.join(",", fields);
    }

    private static String pageShapeSignature(PDDocument document) throws Exception {
        List<String> pages = new ArrayList<>();
        for (int i = 0; i < document.getNumberOfPages(); i++) {
            PDPage page = document.getPage(i);
            pages.add(page.getAnnotations().size() + "/" + resourcesSignature(page.getResources()));
        }
        return String.join(";", pages);
    }

    private static String resourcesSignature(PDResources resources) {
        if (resources == null) {
            return "res=none";
        }

        return "font=" + countNames(resources.getFontNames())
            + ",xobject=" + countNames(resources.getXObjectNames())
            + ",colorspace=" + countNames(resources.getColorSpaceNames())
            + ",extgstate=" + countNames(resources.getExtGStateNames())
            + ",pattern=" + countNames(resources.getPatternNames())
            + ",shading=" + countNames(resources.getShadingNames())
            + ",properties=" + countNames(resources.getPropertiesNames());
    }

    private static int countNames(Iterable<COSName> names) {
        int count = 0;
        for (COSName ignored : names) {
            count++;
        }
        return count;
    }

    private static String imagePixelHash(BufferedImage image) throws Exception {
        MessageDigest digest = MessageDigest.getInstance("SHA-256");
        for (int y = 0; y < image.getHeight(); y++) {
            for (int x = 0; x < image.getWidth(); x++) {
                int argb = image.getRGB(x, y);
                digest.update((byte) (argb >>> 24));
                digest.update((byte) (argb >>> 16));
                digest.update((byte) (argb >>> 8));
                digest.update((byte) argb);
            }
        }
        return HexFormat.of().formatHex(digest.digest()).substring(0, 16);
    }

    private static String imageMetrics(BufferedImage image) {
        int width = image.getWidth();
        int height = image.getHeight();
        int total = width * height;
        int background = image.getRGB(0, 0);
        java.util.HashMap<Integer, Integer> histogram = new java.util.HashMap<>();
        int nonBackground = 0;
        int transparent = 0;
        int dominant = 0;
        for (int y = 0; y < height; y++) {
            for (int x = 0; x < width; x++) {
                int argb = image.getRGB(x, y);
                if (((argb >>> 24) & 0xff) < 8) {
                    transparent++;
                }
                if (colorDistance(argb, background) > 8) {
                    nonBackground++;
                }
                int count = histogram.getOrDefault(argb, 0) + 1;
                histogram.put(argb, count);
                if (count > dominant) {
                    dominant = count;
                }
            }
        }
        boolean nearBlank = transparent == total || nonBackground <= Math.max(10, total / 1000) || dominant >= (int) Math.ceil(total * 0.995);
        return "nonBg=" + nonBackground + ":unique=" + histogram.size() + ":dominant=" + dominant + ":transparent=" + transparent + ":nearBlank=" + nearBlank;
    }

    private static int colorDistance(int a, int b) {
        int aa = (a >>> 24) & 0xff;
        int ar = (a >>> 16) & 0xff;
        int ag = (a >>> 8) & 0xff;
        int ab = a & 0xff;
        int ba = (b >>> 24) & 0xff;
        int br = (b >>> 16) & 0xff;
        int bg = (b >>> 8) & 0xff;
        int bb = b & 0xff;
        return Math.abs(aa - ba) + Math.abs(ar - br) + Math.abs(ag - bg) + Math.abs(ab - bb);
    }

    private static String message(Throwable t) {
        Throwable root = t;
        while (root.getCause() != null) root = root.getCause();
        String msg = root.getClass().getSimpleName() + ":" + (root.getMessage() == null ? "" : root.getMessage());
        return msg.replace('\n', ' ').replace('\r', ' ');
    }

    private static void emit(String file, String op, boolean ok, int pages, String detail, long ms) {
        System.out.println("{\"file\":\"" + esc(file) + "\",\"op\":\"" + op + "\",\"ok\":" + ok
            + ",\"pages\":" + pages + ",\"ms\":" + ms + ",\"detail\":\"" + esc(detail) + "\"}");
    }

    private static String esc(String s) {
        StringBuilder sb = new StringBuilder(s.length());
        for (int i = 0; i < s.length(); i++) {
            char ch = s.charAt(i);
            switch (ch) {
                case '\\':
                    sb.append("\\\\");
                    break;
                case '"':
                    sb.append("\\\"");
                    break;
                case '\b':
                    sb.append("\\b");
                    break;
                case '\f':
                    sb.append("\\f");
                    break;
                case '\n':
                    sb.append("\\n");
                    break;
                case '\r':
                    sb.append("\\r");
                    break;
                case '\t':
                    sb.append("\\t");
                    break;
                default:
                    if (ch < 0x20) {
                        sb.append(String.format("\\u%04x", (int) ch));
                    } else {
                        sb.append(ch);
                    }
                    break;
            }
        }
        return sb.toString();
    }
}
