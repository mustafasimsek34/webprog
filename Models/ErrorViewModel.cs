/*
 * =============================================================================
 * ERROR VIEWMODEL
 * =============================================================================
 *
 * AÇIKLAMA:
 * `ErrorViewModel` hata sayfasında kullanılan basit bir modeldir. RequestId
 * bilgisi ve görüntüleme mantığını içerir.
 *
 * KULLANIM:
 * - `HomeController.Error` metodu tarafından döndürülür.
 *
 * =============================================================================
 */

namespace FitnessCenterManagement.Models;

public class ErrorViewModel
{
    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
}
