﻿namespace Steeltype.QRCoderLite
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

        /// <summary>
        /// Set a QRCodeData object that will be used to generate QR code. Used in COM Objects connections
        /// </summary>
        /// <param name="data">Need a QRCodeData object generated by QRCodeGenerator.CreateQrCode()</param>
        public virtual void SetQRCodeData(QRCodeData data)
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