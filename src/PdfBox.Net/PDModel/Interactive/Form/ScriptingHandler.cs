using PdfBox.Net.PDModel.Interactive.Action;

namespace PdfBox.Net.PDModel.Interactive.Form;

public interface ScriptingHandler
{
    string Keyboard(PDActionJavaScript javaScriptAction, string value);
    string Format(PDActionJavaScript javaScriptAction, string value);
    bool Validate(PDActionJavaScript javaScriptAction, string value);
    string Calculate(PDActionJavaScript javaScriptAction, string value);
}
