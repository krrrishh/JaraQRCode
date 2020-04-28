![](https://raw.githubusercontent.com/jaysonragasa/JaraQRCode/master/JaraQRCode.WPF/prev.gif)
# Jara QRCode
QRCode Generator - A .NET Standard version of Hyun.QRCode originally written for Windows Phone.  
Supports upto 15 characters max.

# Hyun.QRCode
Unfortunately, the original source cannot be found anymore anywhere in Google.

# Usage
```csharp
JaraQRCode.QRCode qr = new QRCode()
{
	//QRCodeBackgroundColor = Color.Black,              // leave it default (default: Black)
	//QRCodeForegroundColor = Color.White,              // leave it default (default: White)
	//QRCodeEncodeMode = QRCode.MODE.BYTE,              // leave it default (defalt: QRCode.MODE.BYTE)
	//QRCodeErrorCorrect = QRCode.ERRORCORRECTION.M,    // leave it default (default: QRCode.ERRORCORRECTION.M)
	//QRCodeScale = 4,                                  // leave it default (4)
	//QRCodeVersion = 0                                 // leave it default (0)
};

var byt = qr.Generate(txText.Text);
if (byt != null)
{
	imgQRCode.Source = Convert(byt);
	txQRstring.Text = qr.GenerateQRCodeString(txText.Text, Encoding.UTF8);
}

public BitmapImage Convert(byte[] src)
{
	MemoryStream ms = new MemoryStream(src);

	BitmapImage image = new BitmapImage();
	image.BeginInit();
	ms.Seek(0, SeekOrigin.Begin);
	image.StreamSource = ms;
	image.EndInit();
	return image;
}
```
