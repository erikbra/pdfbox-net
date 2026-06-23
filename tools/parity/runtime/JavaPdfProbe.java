import java.awt.image.BufferedImage;
import java.io.File;
import java.io.FileInputStream;
import java.io.InputStream;
import java.nio.charset.StandardCharsets;
import java.security.MessageDigest;
import java.util.HexFormat;
import javax.imageio.ImageIO;
import org.apache.pdfbox.Loader;
import org.apache.pdfbox.multipdf.PDFMergerUtility;
import org.apache.pdfbox.pdmodel.PDDocument;
import org.apache.pdfbox.rendering.PDFRenderer;
import org.apache.pdfbox.text.PDFTextStripper;

public final class JavaPdfProbe {
    public static void main(String[] args) throws Exception {
        if (args.length < 2) {
            System.err.println("usage: JavaPdfProbe <out-dir> <pdf> [<pdf>...] | --merge <out-dir> <pdf-a> <pdf-b>");
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
                    emit(name, "render", true, pages, image.getWidth() + "x" + image.getHeight() + ":" + fileSignature(png), elapsed(started));
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
        return s.replace("\\", "\\\\").replace("\"", "\\\"");
    }
}
