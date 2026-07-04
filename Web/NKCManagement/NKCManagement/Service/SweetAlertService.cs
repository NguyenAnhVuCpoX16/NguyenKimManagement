using CurrieTechnologies.Razor.SweetAlert2;

namespace NKCManagement.Service
{
    public class SweetAlertService : ISweetAlertService
    {
        private readonly CurrieTechnologies.Razor.SweetAlert2.SweetAlertService _swal;
        public SweetAlertService(CurrieTechnologies.Razor.SweetAlert2.SweetAlertService swal)
        {
            _swal = swal;
        }

        public async Task Close()
        {
            await _swal.CloseAsync();
        }

        public async Task<bool> Confirm(string message, string title = "")
        {
            var result = await _swal.FireAsync(
               new SweetAlertOptions
               {
                   Title = title,
                   Text = message,
                   Icon = SweetAlertIcon.Question,
                   ShowCancelButton = true
               });

            return !string.IsNullOrEmpty(result.Value);
        }

        public async Task Error(string message, string title = "")
        {
            await _swal.FireAsync(
           "Error",
           message,
           SweetAlertIcon.Error);
        }

        public Task Loading(string message = "Loading...")
        {
            _ = _swal.FireAsync(new SweetAlertOptions
            {
                Html = @$"

                <style>

                    .swal2-popup.custom-loading-popup {{
                        border-radius:24px !important;
                        padding:0 !important;
                        background:rgba(255,255,255,0.95) !important;
                        backdrop-filter:blur(12px);
                        box-shadow:
                            0 10px 30px rgba(0,0,0,0.12),
                            0 4px 12px rgba(0,0,0,0.08);
                    }}

                    .modern-loader {{
                        width:72px;
                        height:72px;
                        border-radius:50%;
                        position:relative;
                        animation:rotate 1.2s linear infinite;
                    }}

                    .modern-loader::before,
                    .modern-loader::after {{
                        content:'';
                        position:absolute;
                        inset:0;
                        border-radius:50%;
                        border:5px solid transparent;
                    }}

                    .modern-loader::before {{
                        border-top-color:#6366f1;
                        border-right-color:#8b5cf6;
                    }}

                    .modern-loader::after {{
                        inset:8px;
                        border-bottom-color:#06b6d4;
                        border-left-color:#3b82f6;
                        animation:rotateReverse .8s linear infinite;
                    }}

                    @keyframes rotate {{
                        100% {{
                            transform:rotate(360deg);
                        }}
                    }}

                    @keyframes rotateReverse {{
                        100% {{
                            transform:rotate(-360deg);
                        }}
                    }}

                </style>

                <div style='
                    display:flex;
                    flex-direction:column;
                    align-items:center;
                    justify-content:center;
                    padding:28px 24px;
                    min-width:280px;
                    font-family:Segoe UI,sans-serif;
                '>

                    <div class='modern-loader'></div>

                    <div style='
                        margin-top:20px;
                        font-size:18px;
                        font-weight:600;
                        color:#1e293b;
                        text-align:center;
                    '>
                        {message}
                    </div>

                    <div style='
                        margin-top:8px;
                        font-size:13px;
                        color:#64748b;
                        text-align:center;
                    '>
                        Please wait a moment...
                    </div>

                </div>
                ",

                Position = SweetAlertPosition.Center,
                ShowConfirmButton = false,
                AllowOutsideClick = false,
                AllowEscapeKey = false,
                Background = "transparent",
                Backdrop = true,

                CustomClass = new SweetAlertCustomClass
                {
                    Popup = "custom-loading-popup"
                }
            });

            return Task.CompletedTask;
        }

        public async Task Success(string message, string title = "")
        {
            await _swal.FireAsync(
                 "Success",
                 message,
                 SweetAlertIcon.Success
            );
        }

        public Task Toast(string message)
        {
            throw new System.NotImplementedException();
        }

        public async Task Warning(string message, string title = "")
        {
            await _swal.FireAsync(
                 "Warning",
                 message,
                 SweetAlertIcon.Warning
            );
        }
    }
}
