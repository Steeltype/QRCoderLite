namespace Steeltype.QRCoderLite
{
    public abstract class AbstractQRCode
    {
        protected QRCodeData QrCodeData { get; private set; }

        protected AbstractQRCode()
        {
        }

        protected AbstractQRCode(QRCodeData data)
        {
            QrCodeData = data;
        }

        public void Dispose()
        {
            QrCodeData?.Dispose();
            QrCodeData = null;
        }
    }
}