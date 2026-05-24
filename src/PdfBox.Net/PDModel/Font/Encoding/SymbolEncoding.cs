/*
 * Copyright (c) 2026 Erik A. Brandstadmoen (C# port modifications/adaptations).
 * Adapted Symbol encoding for PDModel fonts.
 *
 * PORT_MODE: adapted
 */

namespace PdfBox.Net.PDModel.Font.Encoding;

public sealed class SymbolEncoding : Encoding
{
    public static readonly SymbolEncoding INSTANCE = new();

    private SymbolEncoding()
    {
        // Core Symbol mappings commonly used by PDFs.
        AddCharacterEncoding(32, "space");
        AddCharacterEncoding(33, "exclam");
        AddCharacterEncoding(34, "universal");
        AddCharacterEncoding(35, "numbersign");
        AddCharacterEncoding(36, "existential");
        AddCharacterEncoding(37, "percent");
        AddCharacterEncoding(38, "ampersand");
        AddCharacterEncoding(39, "suchthat");
        AddCharacterEncoding(40, "parenleft");
        AddCharacterEncoding(41, "parenright");
        AddCharacterEncoding(42, "asteriskmath");
        AddCharacterEncoding(43, "plus");
        AddCharacterEncoding(44, "comma");
        AddCharacterEncoding(45, "minus");
        AddCharacterEncoding(46, "period");
        AddCharacterEncoding(47, "slash");
        AddCharacterEncoding(48, "zero");
        AddCharacterEncoding(49, "one");
        AddCharacterEncoding(50, "two");
        AddCharacterEncoding(51, "three");
        AddCharacterEncoding(52, "four");
        AddCharacterEncoding(53, "five");
        AddCharacterEncoding(54, "six");
        AddCharacterEncoding(55, "seven");
        AddCharacterEncoding(56, "eight");
        AddCharacterEncoding(57, "nine");
        AddCharacterEncoding(65, "Alpha");
        AddCharacterEncoding(66, "Beta");
        AddCharacterEncoding(67, "Chi");
        AddCharacterEncoding(68, "Delta");
        AddCharacterEncoding(69, "Epsilon");
        AddCharacterEncoding(70, "Phi");
        AddCharacterEncoding(71, "Gamma");
        AddCharacterEncoding(72, "Eta");
        AddCharacterEncoding(73, "Iota");
        AddCharacterEncoding(74, "theta1");
        AddCharacterEncoding(75, "Kappa");
        AddCharacterEncoding(76, "Lambda");
        AddCharacterEncoding(77, "Mu");
        AddCharacterEncoding(78, "Nu");
        AddCharacterEncoding(79, "Omicron");
        AddCharacterEncoding(80, "Pi");
        AddCharacterEncoding(81, "Theta");
        AddCharacterEncoding(82, "Rho");
        AddCharacterEncoding(83, "Sigma");
        AddCharacterEncoding(84, "Tau");
        AddCharacterEncoding(85, "Upsilon");
        AddCharacterEncoding(86, "sigma1");
        AddCharacterEncoding(87, "Omega");
        AddCharacterEncoding(88, "Xi");
        AddCharacterEncoding(89, "Psi");
        AddCharacterEncoding(90, "Zeta");
        AddCharacterEncoding(97, "alpha");
        AddCharacterEncoding(98, "beta");
        AddCharacterEncoding(99, "chi");
        AddCharacterEncoding(100, "delta");
        AddCharacterEncoding(101, "epsilon");
        AddCharacterEncoding(102, "phi");
        AddCharacterEncoding(103, "gamma");
        AddCharacterEncoding(104, "eta");
        AddCharacterEncoding(105, "iota");
        AddCharacterEncoding(106, "phi1");
        AddCharacterEncoding(107, "kappa");
        AddCharacterEncoding(108, "lambda");
        AddCharacterEncoding(109, "mu");
        AddCharacterEncoding(110, "nu");
        AddCharacterEncoding(111, "omicron");
        AddCharacterEncoding(112, "pi");
        AddCharacterEncoding(113, "theta");
        AddCharacterEncoding(114, "rho");
        AddCharacterEncoding(115, "sigma");
        AddCharacterEncoding(116, "tau");
        AddCharacterEncoding(117, "upsilon");
        AddCharacterEncoding(118, "omega1");
        AddCharacterEncoding(119, "omega");
        AddCharacterEncoding(120, "xi");
        AddCharacterEncoding(121, "psi");
        AddCharacterEncoding(122, "zeta");
    }
}
