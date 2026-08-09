namespace DilKursu.Web.Infrastructure.Auditing;

/// <summary>
/// Bir controller action'ının denetlenmesi (audit) gerektiğini belirten işaretleyici öznitelik.
/// "[Modül][Aksiyon]" mantığıyla, işlem tamamlandığında <see cref="AuditActionFilter"/> tarafından
/// otomatik olarak denetim kaydı oluşturulur. Örn: <c>[Audit("Ders", "Ekleme")]</c>.
/// </summary>
/// <param name="module">İşlemin ait olduğu modül (ör. "Ders").</param>
/// <param name="action">Yapılan işlem (ör. "Ekleme").</param>
[AttributeUsage(AttributeTargets.Method)]
public sealed class AuditAttribute(string module, string action) : Attribute
{
    /// <summary>İşlemin ait olduğu modül.</summary>
    public string Module { get; } = module;

    /// <summary>Yapılan işlem.</summary>
    public string Action { get; } = action;
}
