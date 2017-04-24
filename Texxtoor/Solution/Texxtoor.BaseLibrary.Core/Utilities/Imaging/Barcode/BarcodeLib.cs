using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Data;
using System.IO;
using System.Drawing.Imaging;
using Texxtoor.BaseLibrary.Core.Utilities.Imaging.Barcode.Symbologies;

/* 
 * ***************************************************
 *                 Barcode Library                   *
 *                                                   *
 *             Written by: Brad Barnhill             *
 *                   Date: 09-21-2007                *
 *          Last Modified: 12-01-2011                *
 *                                                   *
 *  This library was designed to give developers an  *
 *  easy class to use when they need to generate     *
 *  barcode images from a string of data.            *
 * ***************************************************
 */
namespace Texxtoor.BaseLibrary.Core.Utilities.Imaging.Barcode
{
    #region Enums
    public enum BarCodeType : int { UNSPECIFIED, UPCA, UPCE, UPC_SUPPLEMENTAL_2DIGIT, UPC_SUPPLEMENTAL_5DIGIT, EAN13, EAN8, Interleaved2of5, Standard2of5, Industrial2of5, CODE39, CODE39Extended, Codabar, PostNet, BOOKLAND, ISBN, JAN13, MSI_Mod10, MSI_2Mod10, MSI_Mod11, MSI_Mod11_Mod10, Modified_Plessey, CODE11, USD8, UCC12, UCC13, LOGMARS, CODE128, CODE128A, CODE128B, CODE128C, ITF14, CODE93, TELEPEN, FIM };
    public enum SaveTypes : int { JPG, BMP, PNG, GIF, TIFF, UNSPECIFIED };
    public enum AlignmentPositions : int { CENTER, LEFT, RIGHT};
    public enum LabelPositions : int { TOPLEFT, TOPCENTER, TOPRIGHT, BOTTOMLEFT, BOTTOMCENTER, BOTTOMRIGHT };
    #endregion
    /// <summary>
    /// Generates a barcode image of a specified symbology from a string of data.
    /// </summary>
    public class BarcodeLib: IDisposable
    {
        #region Variables
        private IBarcode _ibarcode = new Blank();
        private string _rawData = "";
        private string _encodedValue = "";
        private string _countryAssigningManufacturerCode = "N/A";
        private BarCodeType _encodedType = BarCodeType.UNSPECIFIED;
        private Image _encodedImage = null;
        private Color _foreColor = Color.Black;
        private Color _backColor = Color.White;
        private int _width = 300;
        private int _height = 150;
        private string _XML = "";
        private ImageFormat _ImageFormat = ImageFormat.Jpeg;
        private Font _LabelFont = new Font("Microsoft Sans Serif", 10, FontStyle.Bold);
        private AlignmentPositions _Alignment = AlignmentPositions.CENTER;
        private LabelPositions _LabelPosition = LabelPositions.BOTTOMCENTER;
        private RotateFlipType _RotateFlipType = RotateFlipType.RotateNoneFlipNone;
        #endregion

        #region Constructors
        /// <summary>
        /// Default constructor.  Does not populate the raw data.  MUST be done via the RawData property before encoding.
        /// </summary>
        public BarcodeLib()
        {
            //constructor
        }//Barcode
        /// <summary>
        /// Constructor. Populates the raw data. No whitespace will be added before or after the barcode.
        /// </summary>
        /// <param name="data">String to be encoded.</param>
        public BarcodeLib(string data)
        {
            //constructor
            this._rawData = data;
        }//Barcode
        public BarcodeLib(string data, BarCodeType iType)
        {
            this._rawData = data;
            this._encodedType = iType;
        }
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets the raw data to encode.
        /// </summary>
        public string RawData
        {
            get { return _rawData; }
            set { _rawData = value; }
        }//RawData
        /// <summary>
        /// Gets the encoded value.
        /// </summary>
        public string EncodedValue
        {
            get { return _encodedValue; }
        }//EncodedValue
        /// <summary>
        /// Gets the Country that assigned the Manufacturer Code.
        /// </summary>
        public string Country_Assigning_Manufacturer_Code
        {
            get { return _countryAssigningManufacturerCode; }
        }//Country_Assigning_Manufacturer_Code
        /// <summary>
        /// Gets or sets the Encoded Type (ex. UPC-A, EAN-13 ... etc)
        /// </summary>
        public BarCodeType EncodedType
        {
            set { _encodedType = value; }
            get { return _encodedType;  }
        }//EncodedType
        /// <summary>
        /// Gets the Image of the generated barcode.
        /// </summary>
        public Image EncodedImage
        {
            get 
            {
                return _encodedImage; 
            }
        }//EncodedImage
        /// <summary>
        /// Gets or sets the color of the bars. (Default is black)
        /// </summary>
        public Color ForeColor
        {
            get { return this._foreColor; }
            set { this._foreColor = value; }
        }//ForeColor
        /// <summary>
        /// Gets or sets the background color. (Default is white)
        /// </summary>
        public Color BackColor
        {
            get { return this._backColor; }
            set { this._backColor = value; }
        }//BackColor
        /// <summary>
        /// Gets or sets the label font. (Default is Microsoft Sans Serif, 10pt, Bold)
        /// </summary>
        public Font LabelFont
        {
            get { return this._LabelFont; }
            set { this._LabelFont = value; }
        }//LabelFont
        /// <summary>
        /// Gets or sets the location of the label in relation to the barcode. (BOTTOMCENTER is default)
        /// </summary>
        public LabelPositions LabelPosition
        {
            get { return _LabelPosition; }
            set { _LabelPosition = value; }
        }//LabelPosition
        /// <summary>
        /// Gets or sets the degree in which to rotate/flip the image.(No action is default)
        /// </summary>
        public RotateFlipType RotateFlipType
        {
            get { return _RotateFlipType; }
            set { _RotateFlipType = value; }
        }//RotatePosition
        /// <summary>
        /// Gets or sets the width of the image to be drawn. (Default is 300 pixels)
        /// </summary>
        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }
        /// <summary>
        /// Gets or sets the height of the image to be drawn. (Default is 150 pixels)
        /// </summary>
        public int Height
        {
            get { return _height; }
            set { _height = value; }
        }
        /// <summary>
        /// Gets or sets whether a label should be drawn below the image. (Default is false)
        /// </summary>
        public bool IncludeLabel
        {
            get;
            set;
        }
        /// <summary>
        /// Gets or sets the amount of time in milliseconds that it took to encode and draw the barcode.
        /// </summary>
        public double EncodingTime
        {
            get;
            set;
        }
        /// <summary>
        /// Gets the XML representation of the Barcode data and image.
        /// </summary>
        public string XML
        {
            get { return _XML; }
        }
        /// <summary>
        /// Gets or sets the image format to use when encoding and returning images. (Jpeg is default)
        /// </summary>
        public ImageFormat ImageFormat
        {
            get { return _ImageFormat; }
            set { _ImageFormat = value; }
        }
        /// <summary>
        /// Gets the list of errors encountered.
        /// </summary>
        public List<string> Errors
        {
            get { return this._ibarcode.Errors; }
        }
        /// <summary>
        /// Gets or sets the alignment of the barcode inside the image. (Not for Postnet or ITF-14)
        /// </summary>
        public AlignmentPositions Alignment
        {
            get;
            set;
        }//Alignment
        /// <summary>
        /// Gets a byte array representation of the encoded image. (Used for Crystal Reports)
        /// </summary>
        public byte[] Encoded_Image_Bytes
        {
            get
            {
                if (_encodedImage == null)
                    return null;
                
                using (MemoryStream ms = new MemoryStream())
                {
                    _encodedImage.Save(ms, _ImageFormat);
                    return ms.ToArray();
                }//using
            }
        }
        /// <summary>
        /// Gets the assembly version information.
        /// </summary>
        public static Version Version
        {
            get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;}
        }
        #endregion

        #region Functions
        #region General Encode
        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="iType">Type of encoding to use.</param>
        /// <param name="StringToEncode">Raw data to encode.</param>
        /// <param name="Width">Width of the resulting barcode.(pixels)</param>
        /// <param name="Height">Height of the resulting barcode.(pixels)</param>
        /// <returns>Image representing the barcode.</returns>
        public Image Encode(BarCodeType iType, string StringToEncode, int Width, int Height)
        {
            this.Width = Width;
            this.Height = Height;
            return Encode(iType, StringToEncode);
        }//Encode(TYPE, string, int, int)
        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="iType">Type of encoding to use.</param>
        /// <param name="StringToEncode">Raw data to encode.</param>
        /// <param name="DrawColor">Foreground color</param>
        /// <param name="BackColor">Background color</param>
        /// <param name="Width">Width of the resulting barcode.(pixels)</param>
        /// <param name="Height">Height of the resulting barcode.(pixels)</param>
        /// <returns>Image representing the barcode.</returns>
        public Image Encode(BarCodeType iType, string StringToEncode, Color ForeColor, Color BackColor, int Width, int Height)
        {
            this.Width = Width;
            this.Height = Height;
            return Encode(iType, StringToEncode, ForeColor, BackColor);
        }//Encode(TYPE, string, Color, Color, int, int)
        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="iType">Type of encoding to use.</param>
        /// <param name="StringToEncode">Raw data to encode.</param>
        /// <param name="DrawColor">Foreground color</param>
        /// <param name="BackColor">Background color</param>
        /// <returns>Image representing the barcode.</returns>
        public Image Encode(BarCodeType iType, string StringToEncode, Color ForeColor, Color BackColor)
        {
            this.BackColor = BackColor;
            this.ForeColor = ForeColor;
            return Encode(iType, StringToEncode);
        }//(Image)Encode(Type, string, Color, Color)
        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="iType">Type of encoding to use.</param>
        /// <param name="StringToEncode">Raw data to encode.</param>
        /// <returns>Image representing the barcode.</returns>
        public Image Encode(BarCodeType iType, string StringToEncode)
        {
            _rawData = StringToEncode;
            return Encode(iType);
        }//(Image)Encode(TYPE, string)
        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="iType">Type of encoding to use.</param>
        public Image Encode(BarCodeType iType)
        {
            _encodedType = iType;
            return Encode();
        }//Encode()
        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.
        /// </summary>
        public Image Encode()
        {
            _ibarcode.Errors.Clear();

            DateTime dtStartTime = DateTime.Now;

            //make sure there is something to encode
            if (_rawData.Trim() == "") 
                throw new Exception("EENCODE-1: Input data not allowed to be blank.");

            if (this.EncodedType == BarCodeType.UNSPECIFIED) 
                throw new Exception("EENCODE-2: Symbology type not allowed to be unspecified.");

            this._encodedValue = "";
            this._countryAssigningManufacturerCode = "N/A";
            
            switch (this._encodedType)
            {
                case BarCodeType.UCC12:
                case BarCodeType.UPCA: //Encode_UPCA();
                    _ibarcode = new UPCA(_rawData);
                    break;
                case BarCodeType.UCC13:
                case BarCodeType.EAN13: //Encode_EAN13();
                    _ibarcode = new EAN13(_rawData);
                    break;
                case BarCodeType.Interleaved2of5: //Encode_Interleaved2of5();
                    _ibarcode = new Interleaved2of5(_rawData);
                    break;
                case BarCodeType.Industrial2of5:
                case BarCodeType.Standard2of5: //Encode_Standard2of5();
                    _ibarcode = new Standard2of5(_rawData);
                    break;
                case BarCodeType.LOGMARS:
                case BarCodeType.CODE39: //Encode_Code39();
                    _ibarcode = new Code39(_rawData);
                    break;
                case BarCodeType.CODE39Extended:
                    _ibarcode = new Code39(_rawData, true);
                    break;
                case BarCodeType.Codabar: //Encode_Codabar();
                    _ibarcode = new Codabar(_rawData);
                    break;
                case BarCodeType.PostNet: //Encode_PostNet();
                    _ibarcode = new Postnet(_rawData);
                    break;
                case BarCodeType.ISBN:
                case BarCodeType.BOOKLAND: //Encode_ISBN_Bookland();
                    _ibarcode = new ISBN(_rawData);
                    break;
                case BarCodeType.JAN13: //Encode_JAN13();
                    _ibarcode = new JAN13(_rawData);
                    break;
                case BarCodeType.UPC_SUPPLEMENTAL_2DIGIT: //Encode_UPCSupplemental_2();
                    _ibarcode = new UPCSupplement2(_rawData);
                    break;
                case BarCodeType.MSI_Mod10:
                case BarCodeType.MSI_2Mod10:
                case BarCodeType.MSI_Mod11:
                case BarCodeType.MSI_Mod11_Mod10:
                case BarCodeType.Modified_Plessey: //Encode_MSI();
                    _ibarcode = new MSI(_rawData, _encodedType);
                    break;
                case BarCodeType.UPC_SUPPLEMENTAL_5DIGIT: //Encode_UPCSupplemental_5();
                    _ibarcode = new UPCSupplement5(_rawData);
                    break;
                case BarCodeType.UPCE: //Encode_UPCE();
                    _ibarcode = new UPCE(_rawData);
                    break;
                case BarCodeType.EAN8: //Encode_EAN8();
                    _ibarcode = new EAN8(_rawData);
                    break;
                case BarCodeType.USD8:
                case BarCodeType.CODE11: //Encode_Code11();
                    _ibarcode = new Code11(_rawData);
                    break;
                case BarCodeType.CODE128: //Encode_Code128();
                    _ibarcode = new Code128(_rawData);
                    break;
                case BarCodeType.CODE128A:
                    _ibarcode = new Code128(_rawData, Code128.TYPES.A);
                    break;
                case BarCodeType.CODE128B:
                    _ibarcode = new Code128(_rawData, Code128.TYPES.B);
                    break;
                case BarCodeType.CODE128C:
                    _ibarcode = new Code128(_rawData, Code128.TYPES.C);
                    break;
                case BarCodeType.ITF14:
                    _ibarcode = new ITF14(_rawData);
                    break;
                case BarCodeType.CODE93:
                    _ibarcode = new Code93(_rawData);
                    break;
                case BarCodeType.TELEPEN:
                    _ibarcode = new Telepen(_rawData);
                    break;
                case BarCodeType.FIM:
                    _ibarcode = new FIM(_rawData);
                    break;
                default: throw new Exception("EENCODE-2: Unsupported encoding type specified.");
            }//switch

            this._encodedValue = _ibarcode.Encoded_Value;
            this._rawData = _ibarcode.RawData;

            _encodedImage = (Image)Generate_Image();

            this.EncodedImage.RotateFlip(this.RotateFlipType);

            _XML = GetXML();

            this.EncodingTime = ((TimeSpan)(DateTime.Now - dtStartTime)).TotalMilliseconds;
            
            return EncodedImage;
        }//Encode
        #endregion

        #region Image Functions
        /// <summary>
        /// Gets a bitmap representation of the encoded data.
        /// </summary>
        /// <returns>Bitmap of encoded value.</returns>
        private Bitmap Generate_Image()
        {
            if (_encodedValue == "") throw new Exception("EGENERATE_IMAGE-1: Must be encoded first.");
            Bitmap b = null;

            DateTime dtStartTime = DateTime.Now;

            switch (this._encodedType)
            {
                case BarCodeType.ITF14:
                    {
                        b = new Bitmap(Width, Height);

                        int bearerwidth = (int)((b.Width) / 12.05);
                        int iquietzone = Convert.ToInt32(b.Width * 0.05);
                        int iBarWidth = (b.Width - (bearerwidth * 2) - (iquietzone * 2)) / _encodedValue.Length;
                        int shiftAdjustment = ((b.Width - (bearerwidth * 2) - (iquietzone * 2)) % _encodedValue.Length) / 2;

                        if (iBarWidth <= 0 || iquietzone <= 0)
                            throw new Exception("EGENERATE_IMAGE-3: Image size specified not large enough to draw image. (Bar size determined to be less than 1 pixel or quiet zone determined to be less than 1 pixel)");

                        //draw image
                        int pos = 0;

                        using (Graphics g = Graphics.FromImage(b))
                        {
                            //fill background
                            g.Clear(BackColor);

                            //lines are fBarWidth wide so draw the appropriate color line vertically
                            using (Pen pen = new Pen(ForeColor, iBarWidth))
                            {
                                pen.Alignment = PenAlignment.Right;

                                while (pos < _encodedValue.Length)
                                {
                                    //draw the appropriate color line vertically
                                    if (_encodedValue[pos] == '1')
                                        g.DrawLine(pen, new Point((pos * iBarWidth) + shiftAdjustment + bearerwidth + iquietzone, 0), new Point((pos * iBarWidth) + shiftAdjustment + bearerwidth + iquietzone, Height));

                                    pos++;
                                }//while

                                //bearer bars
                                pen.Width = (float)b.Height / 8;
                                pen.Color = ForeColor;
                                pen.Alignment = PenAlignment.Center;
                                g.DrawLine(pen, new Point(0, 0), new Point(b.Width, 0));//top
                                g.DrawLine(pen, new Point(0, b.Height), new Point(b.Width, b.Height));//bottom
                                g.DrawLine(pen, new Point(0, 0), new Point(0, b.Height));//left
                                g.DrawLine(pen, new Point(b.Width, 0), new Point(b.Width, b.Height));//right
                            }//using
                        }//using

                        if (IncludeLabel)
                            Label_ITF14((Image)b);

                        break;
                    }//case
                default:
                    {
                        b = new Bitmap(Width, Height);
                        int iBarWidth = Width / _encodedValue.Length;
                        int shiftAdjustment = 0;
                        int iBarWidthModifier = 1;

                        if (this._encodedType == BarCodeType.PostNet)
                            iBarWidthModifier = 2;

                        //set alignment
                        switch (Alignment)
                        {
                            case AlignmentPositions.CENTER: shiftAdjustment = (Width % _encodedValue.Length) / 2;
                                break;
                            case AlignmentPositions.LEFT: shiftAdjustment = 0;
                                break;
                            case AlignmentPositions.RIGHT: shiftAdjustment = (Width % _encodedValue.Length);
                                break;
                            default: shiftAdjustment = (Width % _encodedValue.Length) / 2;
                                break;
                        }//switch

                        if (iBarWidth <= 0)
                            throw new Exception("EGENERATE_IMAGE-2: Image size specified not large enough to draw image. (Bar size determined to be less than 1 pixel)");

                        //draw image
                        int pos = 0;

                        using (Graphics g = Graphics.FromImage(b))
                        {
                            //clears the image and colors the entire background
                            g.Clear(BackColor);

                            //lines are fBarWidth wide so draw the appropriate color line vertically
                            using (Pen backpen = new Pen(BackColor, iBarWidth / iBarWidthModifier))
                            {
                                using (Pen pen = new Pen(ForeColor, iBarWidth / iBarWidthModifier))
                                {
                                    while (pos < _encodedValue.Length)
                                    {
                                        if (this._encodedType == BarCodeType.PostNet)
                                        {
                                            //draw half bars in postnet
                                            if (_encodedValue[pos] != '1')
                                                g.DrawLine(pen, new Point(pos * iBarWidth + shiftAdjustment + 1, Height), new Point(pos * iBarWidth + shiftAdjustment + 1, Height / 2));
                                            
                                            //draw spaces between bars in postnet
                                            g.DrawLine(backpen, new Point(pos * (iBarWidth * iBarWidthModifier) + shiftAdjustment + iBarWidth + 1, 0), new Point(pos * (iBarWidth * iBarWidthModifier) + shiftAdjustment + iBarWidth + 1, Height));
                                        }//if

                                        if (_encodedValue[pos] == '1')
                                            g.DrawLine(pen, new Point(pos * iBarWidth + shiftAdjustment + (int)(iBarWidth * 0.5), 0), new Point(pos * iBarWidth + shiftAdjustment + (int)(iBarWidth * 0.5), Height));

                                        pos++;
                                    }//while
                                }//using
                            }//using
                        }//using
                        if (IncludeLabel)
                        {
                            //if (this.EncodedType != TYPE.UPCA)
                                Label_Generic((Image)b);
                            //else
                            //    Label_UPCA((Image)b);
                        }//if
                        
                        break;
                    }//case
            }//switch

            _encodedImage = (Image)b;

            this.EncodingTime += ((TimeSpan)(DateTime.Now - dtStartTime)).TotalMilliseconds;

            return b;
        }//Generate_Image
        /// <summary>
        /// Gets the bytes that represent the image.
        /// </summary>
        /// <param name="savetype">File type to put the data in before returning the bytes.</param>
        /// <returns>Bytes representing the encoded image.</returns>
        public byte[] GetImageData(SaveTypes savetype)
        {
            byte[] imageData = null;
              
            try
            {
                if (_encodedImage != null)
                {
                    //Save the image to a memory stream so that we can get a byte array!      
                    using (MemoryStream ms = new MemoryStream())
                    {
                        SaveImage(ms, savetype);
                        imageData = ms.ToArray();
                        ms.Flush();
                        ms.Close();
                    }//using
                }//if
            }//try
            catch (Exception ex)
            {
                throw new Exception("EGETIMAGEDATA-1: Could not retrieve image data. " + ex.Message);
            }//catch  
            return imageData;
        }
        /// <summary>
        /// Saves an encoded image to a specified file and type.
        /// </summary>
        /// <param name="Filename">Filename to save to.</param>
        /// <param name="FileType">Format to use.</param>
        public void SaveImage(string Filename, SaveTypes FileType)
        {
            try
            {
                if (_encodedImage != null)
                {
                    System.Drawing.Imaging.ImageFormat imageformat;
                    switch (FileType)
                    {
                        case SaveTypes.BMP: imageformat = System.Drawing.Imaging.ImageFormat.Bmp; break;
                        case SaveTypes.GIF: imageformat = System.Drawing.Imaging.ImageFormat.Gif; break;
                        case SaveTypes.JPG: imageformat = System.Drawing.Imaging.ImageFormat.Jpeg; break;
                        case SaveTypes.PNG: imageformat = System.Drawing.Imaging.ImageFormat.Png; break;
                        case SaveTypes.TIFF: imageformat = System.Drawing.Imaging.ImageFormat.Tiff; break;
                        default: imageformat = ImageFormat; break;
                    }//switch
                    ((Bitmap)_encodedImage).Save(Filename, imageformat);
                }//if
            }//try
            catch(Exception ex)
            {
                throw new Exception("ESAVEIMAGE-1: Could not save image.\n\n=======================\n\n" + ex.Message);
            }//catch
        }//SaveImage(string, SaveTypes)
        /// <summary>
        /// Saves an encoded image to a specified stream.
        /// </summary>
        /// <param name="stream">Stream to write image to.</param>
        /// <param name="FileType">Format to use.</param>
        public void SaveImage(Stream stream, SaveTypes FileType)
        {
            try
            {
                if (_encodedImage != null)
                {
                    System.Drawing.Imaging.ImageFormat imageformat;
                    switch (FileType)
                    {
                        case SaveTypes.BMP: imageformat = System.Drawing.Imaging.ImageFormat.Bmp; break;
                        case SaveTypes.GIF: imageformat = System.Drawing.Imaging.ImageFormat.Gif; break;
                        case SaveTypes.JPG: imageformat = System.Drawing.Imaging.ImageFormat.Jpeg; break;
                        case SaveTypes.PNG: imageformat = System.Drawing.Imaging.ImageFormat.Png; break;
                        case SaveTypes.TIFF: imageformat = System.Drawing.Imaging.ImageFormat.Tiff; break;
                        default: imageformat = ImageFormat; break;
                    }//switch
                    ((Bitmap)_encodedImage).Save(stream, imageformat);
                }//if
            }//try
            catch (Exception ex)
            {
                throw new Exception("ESAVEIMAGE-2: Could not save image.\n\n=======================\n\n" + ex.Message);
            }//catch
        }//SaveImage(Stream, SaveTypes)
        /// <summary>
        /// Returns the size of the EncodedImage in real world coordinates (millimeters or inches).
        /// </summary>
        /// <param name="Width">Width of the image in the specified coordinates.</param>
        /// <param name="Height">Height of the image in the specified coordinates.</param>
        /// <param name="Metric">Millimeters if true, otherwise Inches.</param>
        /// <returns></returns>
        public void GetSizeOfImage(ref double Width, ref double Height, bool Metric)
        {
            Width = 0;
            Height = 0;
            if (this.EncodedImage != null && this.EncodedImage.Width > 0 && this.EncodedImage.Height > 0)
            {
                double MillimetersPerInch = 25.4;
                using (Graphics g = Graphics.FromImage(this.EncodedImage))
                {
                    Width = this.EncodedImage.Width / g.DpiX;
                    Height = this.EncodedImage.Height / g.DpiY;

                    if (Metric)
                    {
                        Width *= MillimetersPerInch;
                        Height *= MillimetersPerInch;
                    }//if
                }//using
            }//if
        }
        #endregion
        
        #region Label Generation
        private Image Label_ITF14(Image img)
        {
            try
            {
                System.Drawing.Font font = this.LabelFont;

                using (Graphics g = Graphics.FromImage(img))
                {
                    g.DrawImage(img, (float)0, (float)0);

                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.HighQuality;

                    //color a white box at the bottom of the barcode to hold the string of data
                    g.FillRectangle(new SolidBrush(this.BackColor), new Rectangle(0, img.Height - (font.Height - 2), img.Width, font.Height));

                    //draw datastring under the barcode image
                    StringFormat f = new StringFormat();
                    f.Alignment = StringAlignment.Center;
                    g.DrawString(this.RawData, font, new SolidBrush(this.ForeColor), (float)(img.Width / 2), img.Height - font.Height + 1, f);

                    Pen pen = new Pen(ForeColor, (float)img.Height / 16);
                    pen.Alignment = PenAlignment.Inset;
                    g.DrawLine(pen, new Point(0, img.Height - font.Height - 2), new Point(img.Width, img.Height - font.Height - 2));//bottom

                    g.Save();
                }//using
                return img;
            }//try
            catch (Exception ex)
            {
                throw new Exception("ELABEL_ITF14-1: " + ex.Message);
            }//catch
        }
        private Image Label_Generic(Image img)
        {
            try
            {
                System.Drawing.Font font = this.LabelFont;

                using (Graphics g = Graphics.FromImage(img))
                {
                    g.DrawImage(img, (float)0, (float)0);

                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.HighQuality;

                    StringFormat f = new StringFormat();
                    f.Alignment = StringAlignment.Near;
                    f.LineAlignment = StringAlignment.Near;
                    int LabelX = 0;
                    int LabelY = 0;

                    switch (LabelPosition)
                    {
                        case LabelPositions.BOTTOMCENTER:
                            LabelX = img.Width / 2;
                            LabelY = img.Height - (font.Height);
                            f.Alignment = StringAlignment.Center;
                            break;
                        case LabelPositions.BOTTOMLEFT:
                            LabelX = 0;
                            LabelY = img.Height - (font.Height);
                            f.Alignment = StringAlignment.Near;
                            break;
                        case LabelPositions.BOTTOMRIGHT:
                            LabelX = img.Width;
                            LabelY = img.Height - (font.Height);
                            f.Alignment = StringAlignment.Far;
                            break;
                        case LabelPositions.TOPCENTER:
                            LabelX = img.Width / 2;
                            LabelY = 0;
                            f.Alignment = StringAlignment.Center;
                            break;
                        case LabelPositions.TOPLEFT:
                            LabelX = img.Width;
                            LabelY = 0;
                            f.Alignment = StringAlignment.Near;
                            break;
                        case LabelPositions.TOPRIGHT:
                            LabelX = img.Width;
                            LabelY = 0;
                            f.Alignment = StringAlignment.Far;
                            break;
                    }//switch
                    
                    //color a background color box at the bottom of the barcode to hold the string of data
                    g.FillRectangle(new SolidBrush(this.BackColor), new RectangleF((float)0, (float)LabelY, (float)img.Width, (float)font.Height));

                    //draw datastring under the barcode image
                    g.DrawString(this.RawData, font, new SolidBrush(this.ForeColor), new RectangleF((float)0, (float)LabelY, (float)img.Width, (float)font.Height), f);

                    g.Save();
                }//using
                return img;
            }//try
            catch (Exception ex)
            {
                throw new Exception("ELABEL_GENERIC-1: " + ex.Message);
            }//catch
        }//Label_Generic
        
        /// <summary>
        /// Draws Label for UPC-A barcodes (NOT COMPLETE)
        /// </summary>
        /// <param name="img"></param>
        /// <returns></returns>
        private Image Label_UPCA(Image img)
        {
            try
            {
                int iBarWidth = Width / _encodedValue.Length;
                int shiftAdjustment = 0;
                
                //set alignment
                switch (Alignment)
                {
                    case AlignmentPositions.CENTER: shiftAdjustment = (Width % _encodedValue.Length) / 2;
                        break;
                    case AlignmentPositions.LEFT: shiftAdjustment = 0;
                        break;
                    case AlignmentPositions.RIGHT: shiftAdjustment = (Width % _encodedValue.Length);
                        break;
                    default: shiftAdjustment = (Width % _encodedValue.Length) / 2;
                        break;
                }//switch

                System.Drawing.Font font = new System.Drawing.Font("OCR A Extended", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0))); ;

                using (Graphics g = Graphics.FromImage(img))
                {
                    g.DrawImage(img, (float)0, (float)0);

                    g.SmoothingMode = SmoothingMode.HighQuality;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    g.CompositingQuality = CompositingQuality.HighQuality;

                    //draw datastring under the barcode image
                    RectangleF rect = new RectangleF((iBarWidth * 3) + shiftAdjustment, this.Height - (int)(this.Height * 0.1), (iBarWidth * 43), (int)(this.Height * 0.1));
                    g.FillRectangle(new SolidBrush(Color.Yellow), rect.X, rect.Y, rect.Width, rect.Height);
                    g.DrawString(this.RawData.Substring(1, 5), font, new SolidBrush(this.ForeColor), rect.X, rect.Y);

                    g.Save();
                }//using
                return img;
            }//try
            catch (Exception ex)
            {
                throw new Exception("ELABEL_UPCA-1: " + ex.Message);
            }//catch
        }//Label_UPCA
        #endregion
        #endregion

        #region Misc
        internal static bool CheckNumericOnly(string Data)
        {
            //This function takes a string of data and breaks it into parts and trys to do Int64.TryParse
            //This will verify that only numeric data is contained in the string passed in.  The complexity below
            //was done to ensure that the minimum number of interations and checks could be performed.

            //9223372036854775808 is the largest number a 64bit number(signed) can hold so ... make sure its less than that by one place
            int STRING_LENGTHS = 18;
            
            string temp = Data;
            string [] strings = new string[(Data.Length / STRING_LENGTHS) + ((Data.Length % STRING_LENGTHS == 0) ? 0 : 1)];
            
            int i = 0;
            while (i < strings.Length)
                if (temp.Length >= STRING_LENGTHS)
                {
                    strings[i++] = temp.Substring(0, STRING_LENGTHS);
                    temp = temp.Substring(STRING_LENGTHS);
                }//if
                else
                    strings[i++] = temp.Substring(0);

            foreach (string s in strings)
            {
                long value = 0;
                if (!Int64.TryParse(s, out value))
                    return false;
            }//foreach

            return true;
        }//CheckNumericOnly
        private string GetXML()
        {
            if (EncodedValue == "")
                throw new Exception("EGETXML-1: Could not retrieve XML due to the barcode not being encoded first.  Please call Encode first.");
            else
            {
                try
                {
                    using (BarcodeXML xml = new BarcodeXML())
                    {
                        BarcodeXML.BarcodeRow row = xml.Barcode.NewBarcodeRow();
                        row.Type = EncodedType.ToString();
                        row.RawData = RawData;
                        row.EncodedValue = EncodedValue;
                        row.EncodingTime = EncodingTime;
                        row.IncludeLabel = IncludeLabel;
                        row.Forecolor = ColorTranslator.ToHtml(ForeColor);
                        row.Backcolor = ColorTranslator.ToHtml(BackColor);
                        row.CountryAssigningManufacturingCode = Country_Assigning_Manufacturer_Code;
                        row.ImageWidth = Width;
                        row.ImageHeight = Height;
                        row.RotateFlipType = this.RotateFlipType;
                        row.LabelPosition = (int)this.LabelPosition;
                        row.LabelFont = this.LabelFont.ToString();
                        row.ImageFormat = this.ImageFormat.ToString();
                        row.Alignment = (int)this.Alignment;

                        //get image in base 64
                        using (MemoryStream ms = new MemoryStream())
                        {
                            EncodedImage.Save(ms, ImageFormat);
                            row.Image = Convert.ToBase64String(ms.ToArray(), Base64FormattingOptions.None);
                        }//using

                        xml.Barcode.AddBarcodeRow(row);

                        StringWriter sw = new StringWriter();
                        xml.WriteXml(sw, XmlWriteMode.WriteSchema);
                        return sw.ToString();
                    }//using
                }//try
                catch (Exception ex)
                {
                    throw new Exception("EGETXML-2: " + ex.Message);
                }//catch
            }//else
        }
        public static Image GetImageFromXML(BarcodeXML internalXML)
        {
            try
            {
                //converting the base64 string to byte array
                Byte[] imageContent = new Byte[internalXML.Barcode[0].Image.Length];

                //loading it to memory stream and then to image object
                using (MemoryStream ms = new MemoryStream(Convert.FromBase64String(internalXML.Barcode[0].Image)))
                {
                    return Image.FromStream(ms);
                }//using
            }//try
            catch (Exception ex)
            {
                throw new Exception("EGETIMAGEFROMXML-1: " + ex.Message);
            }//catch
        }
        #endregion

        #region Static Methods
        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="iType">Type of encoding to use.</param>
        /// <param name="Data">Raw data to encode.</param>
        /// <returns>Image representing the barcode.</returns>
        public static Image DoEncode(BarCodeType iType, string Data)
        {
          using (BarcodeLib b = new BarcodeLib())
            {
                return b.Encode(iType, Data);
            }//using
        }
        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="iType">Type of encoding to use.</param>
        /// <param name="Data">Raw data to encode.</param>
        /// <param name="XML">XML representation of the data and the image of the barcode.</param>
        /// <returns>Image representing the barcode.</returns>
        public static Image DoEncode(BarCodeType iType, string Data, ref string XML)
        {
          using (BarcodeLib b = new BarcodeLib())
            {
                Image i = b.Encode(iType, Data);
                XML = b.XML;
                return i;
            }//using
        }
        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="iType">Type of encoding to use.</param>
        /// <param name="Data">Raw data to encode.</param>
        /// <param name="IncludeLabel">Include the label at the bottom of the image with data encoded.</param>
        /// <returns>Image representing the barcode.</returns>
        public static Image DoEncode(BarCodeType iType, string Data, bool IncludeLabel)
        {
          using (BarcodeLib b = new BarcodeLib())
            {
                b.IncludeLabel = IncludeLabel;
                return b.Encode(iType, Data);
            }//using
        }
        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="iType">Type of encoding to use.</param>
        /// <param name="data">Raw data to encode.</param>
        /// <param name="IncludeLabel">Include the label at the bottom of the image with data encoded.</param>
        /// <param name="Width">Width of the resulting barcode.(pixels)</param>
        /// <param name="Height">Height of the resulting barcode.(pixels)</param>
        /// <returns>Image representing the barcode.</returns>
        public static Image DoEncode(BarCodeType iType, string Data, bool IncludeLabel, int Width, int Height)
        {
          using (BarcodeLib b = new BarcodeLib())
            {
                b.IncludeLabel = IncludeLabel;
                return b.Encode(iType, Data, Width, Height);
            }//using
        }
        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="iType">Type of encoding to use.</param>
        /// <param name="Data">Raw data to encode.</param>
        /// <param name="IncludeLabel">Include the label at the bottom of the image with data encoded.</param>
        /// <param name="DrawColor">Foreground color</param>
        /// <param name="BackColor">Background color</param>
        /// <returns>Image representing the barcode.</returns>
        public static Image DoEncode(BarCodeType iType, string Data, bool IncludeLabel, Color DrawColor, Color BackColor)
        {
          using (BarcodeLib b = new BarcodeLib())
            {
                b.IncludeLabel = IncludeLabel;
                return b.Encode(iType, Data, DrawColor, BackColor);
            }//using
        }
        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="iType">Type of encoding to use.</param>
        /// <param name="Data">Raw data to encode.</param>
        /// <param name="IncludeLabel">Include the label at the bottom of the image with data encoded.</param>
        /// <param name="DrawColor">Foreground color</param>
        /// <param name="BackColor">Background color</param>
        /// <param name="Width">Width of the resulting barcode.(pixels)</param>
        /// <param name="Height">Height of the resulting barcode.(pixels)</param>
        /// <returns>Image representing the barcode.</returns>
        public static Image DoEncode(BarCodeType iType, string Data, bool IncludeLabel, Color DrawColor, Color BackColor, int Width, int Height)
        {
          using (BarcodeLib b = new BarcodeLib())
            {
                b.IncludeLabel = IncludeLabel;
                return b.Encode(iType, Data, DrawColor, BackColor, Width, Height);
            }//using
        }
        /// <summary>
        /// Encodes the raw data into binary form representing bars and spaces.  Also generates an Image of the barcode.
        /// </summary>
        /// <param name="iType">Type of encoding to use.</param>
        /// <param name="Data">Raw data to encode.</param>
        /// <param name="IncludeLabel">Include the label at the bottom of the image with data encoded.</param>
        /// <param name="DrawColor">Foreground color</param>
        /// <param name="BackColor">Background color</param>
        /// <param name="Width">Width of the resulting barcode.(pixels)</param>
        /// <param name="Height">Height of the resulting barcode.(pixels)</param>
        /// <param name="XML">XML representation of the data and the image of the barcode.</param>
        /// <returns>Image representing the barcode.</returns>
        public static Image DoEncode(BarCodeType iType, string Data, bool IncludeLabel, Color DrawColor, Color BackColor, int Width, int Height, ref string XML)
        {
          using (BarcodeLib b = new BarcodeLib())
            {
                b.IncludeLabel = IncludeLabel;
                Image i = b.Encode(iType, Data, DrawColor, BackColor, Width, Height);
                XML = b.XML;
                return i;
            }//using
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
            try
            {
            }//try
            catch (Exception ex)
            {
                throw new Exception("EDISPOSE-1: " + ex.Message);
            }//catch
        }

        #endregion
    }//Barcode Class
}//Barcode namespace
