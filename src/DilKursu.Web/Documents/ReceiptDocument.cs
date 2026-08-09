using DilKursu.Business.Dtos;
using DilKursu.Entities.Enums;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace DilKursu.Web.Documents;

public class ReceiptDocument(ReceiptDto receipt) : IDocument
{
    // Kurumsal marka renkleri (site temasıyla uyumlu).
    private const string BrandColor = "#6366F1";
    private const string LightGray = "#F4F5FB";

    /// <summary>Belge meta verisini döndürür (varsayılan).</summary>
    public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

    /// <summary>
    /// Makbuz sayfasının genel düzenini (boyut, kenar boşluğu, başlık/içerik/altbilgi) oluşturur.
    /// </summary>
    /// <param name="container">Belge kapsayıcısı.</param>
    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A5);
            page.Margin(28);
            page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

            page.Header().Element(ComposeHeader);
            page.Content().PaddingVertical(15).Element(ComposeContent);
            page.Footer().Element(ComposeFooter);
        });
    }

    /// <summary>
    /// Makbuz başlığını (kurum adı, "ÖDEME MAKBUZU" etiketi, makbuz no ve tarih) oluşturur.
    /// </summary>
    /// <param name="container">Başlık kapsayıcısı.</param>
    private void ComposeHeader(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text("Bir Lisan Bir İnsan").FontSize(18).Bold().FontColor(BrandColor);
                    c.Item().Text("Dil Kursu Otomasyon Sistemi").FontSize(9).FontColor(Colors.Grey.Medium);
                });

                row.ConstantItem(150).Column(c =>
                {
                    c.Item().AlignRight().Text("ÖDEME MAKBUZU").FontSize(13).Bold();
                    c.Item().AlignRight().Text(receipt.ReceiptNo).FontSize(10).FontColor(Colors.Grey.Medium);
                    c.Item().AlignRight().Text($"Tarih: {receipt.IssueDate:dd.MM.yyyy}").FontSize(9).FontColor(Colors.Grey.Medium);
                });
            });

            // Marka renginde ince ayraç çizgi.
            col.Item().PaddingTop(8).LineHorizontal(2).LineColor(BrandColor);
        });
    }

    /// <summary>
    /// Makbuz gövdesini (öğrenci/ders bilgileri, tahsil edilen taksit ve ödeme özeti) oluşturur.
    /// </summary>
    /// <param name="container">İçerik kapsayıcısı.</param>
    private void ComposeContent(IContainer container)
    {
        container.Column(col =>
        {
            col.Spacing(10);

            // Öğrenci ve ders bilgileri.
            col.Item().Background(LightGray).Padding(12).Column(c =>
            {
                c.Spacing(4);
                InfoRow(c, "Öğrenci", receipt.StudentName);
                InfoRow(c, "Telefon", string.IsNullOrWhiteSpace(receipt.StudentPhone) ? "-" : receipt.StudentPhone);
                InfoRow(c, "Ders", receipt.CourseInfo);
                InfoRow(c, "Ödeme Türü", receipt.PaymentType == OdemeTuru.Pesin ? "Peşin" : "Taksitli");
            });

            // Tahsil edilen taksit satırı (vurgulu).
            col.Item().Border(1).BorderColor(BrandColor).Padding(12).Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text($"{receipt.InstallmentNo}. Taksit Tahsilatı").Bold();
                    c.Item().Text($"Ödeme Tarihi: {receipt.PaidDate:dd.MM.yyyy}").FontSize(9).FontColor(Colors.Grey.Medium);
                });
                row.ConstantItem(120).AlignRight().AlignMiddle()
                    .Text($"{receipt.InstallmentAmount:N2} ₺").FontSize(16).Bold().FontColor(BrandColor);
            });

            // Ödeme özeti.
            col.Item().PaddingTop(4).Column(c =>
            {
                c.Spacing(3);
                SummaryRow(c, "Toplam Tutar", $"{receipt.TotalAmount:N2} ₺", false);
                SummaryRow(c, "Ödenen Toplam", $"{receipt.PaidAmount:N2} ₺", false);
                SummaryRow(c, "Kalan Tutar", $"{receipt.RemainingAmount:N2} ₺", true);
            });
        });
    }

    /// <summary>
    /// Makbuz altbilgisini (bilgilendirme notu ve sayfa numarası) oluşturur.
    /// </summary>
    /// <param name="container">Altbilgi kapsayıcısı.</param>
    private void ComposeFooter(IContainer container)
    {
        container.Column(col =>
        {
            col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
            col.Item().PaddingTop(5).Text(text =>
            {
                text.Span("Bu makbuz elektronik olarak oluşturulmuştur. ")
                    .FontSize(8).FontColor(Colors.Grey.Medium);
                text.Span($"© {DateTime.Now.Year} Bir Lisan Bir İnsan")
                    .FontSize(8).FontColor(Colors.Grey.Medium);
            });
        });
    }

    /// <summary>
    /// Etiket-değer biçiminde tek bir bilgi satırı çizer (ortak yardımcı, DRY).
    /// </summary>
    private static void InfoRow(ColumnDescriptor col, string label, string value)
    {
        col.Item().Row(row =>
        {
            row.ConstantItem(100).Text(label).FontColor(Colors.Grey.Darken1);
            row.RelativeItem().Text(value).Bold();
        });
    }

    /// <summary>
    /// Ödeme özetinde tek bir satır çizer; <paramref name="highlight"/> true ise vurgular.
    /// </summary>
    private static void SummaryRow(ColumnDescriptor col, string label, string value, bool highlight)
    {
        col.Item().Row(row =>
        {
            row.RelativeItem().Text(label).FontColor(highlight ? Colors.Red.Medium : Colors.Grey.Darken1);
            row.ConstantItem(120).AlignRight().Text(value).Bold()
                .FontColor(highlight ? Colors.Red.Medium : Colors.Black);
        });
    }
}
