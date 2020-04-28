﻿/*
 * I forgot who was the author and the original code cannot find
 * in google anymore. This was originally written for Windows Phone
 * and I converted it to .NET Standard
 * 
 * (c) JaraIO 2020
 */

using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;

namespace JaraQRCode
{
    public class QRCode
    {
        #region enums
        public enum MODE { ALPHA_NUMERIC, NUMERIC, BYTE };
        public enum ERRORCORRECTION { L, M, Q, H };
        #endregion

        #region fields
        internal int _qrCodeVersion;

        internal int _qrcodeStructureappendN;
        internal int _qrcodeStructureappendM;
        internal int _qrcodeStructureappendParity;

        // if you want to limit the version
        const int _maxQrCodeVersionAvailable = 40;
        // if version == 0, we calculate the best version using the step
        const int _qrCodeVersionStep = 1;
        #endregion

        #region props
        public ERRORCORRECTION QRCodeErrorCorrect { get; set; } = ERRORCORRECTION.M;
        public MODE QRCodeEncodeMode { get; set; } = MODE.BYTE;
        public int QRCodeVersion
        {
            get
            {
                return _qrCodeVersion;
            }
            set
            {
                if (value >= 0 && value <= _maxQrCodeVersionAvailable)
                {
                    _qrCodeVersion = value;
                }
            }
        }
        public int QRCodeScale { get; set; } = 4;
        public Color QRCodeBackgroundColor { get; set; } = Color.White;
        public Color QRCodeForegroundColor { get; set; } = Color.Black;
        #endregion

        #region ctor
        public QRCode()
        {
            _qrcodeStructureappendN = 0;
            _qrcodeStructureappendM = 0;
            _qrcodeStructureappendParity = 0;
        }
        #endregion

        #region methods

        private bool[][] CalQrcode(byte[] qrcodeData)
        {
            int dataCounter = 0;
            int dataLength = qrcodeData.Length;

            var dataValue = new int[dataLength + 32];
            sbyte[] dataBits = new sbyte[dataLength + 32];

            if (dataLength <= 0)
            {
                return new bool[][] { new bool[1] };
            }

            if (_qrcodeStructureappendN > 1)
            {
                dataValue[0] = 3;
                dataBits[0] = 4;

                dataValue[1] = _qrcodeStructureappendM - 1;
                dataBits[1] = 4;

                dataValue[2] = _qrcodeStructureappendN - 1;
                dataBits[2] = 4;

                dataValue[3] = _qrcodeStructureappendParity;
                dataBits[3] = 8;

                dataCounter = 4;
            }
            dataBits[dataCounter] = 4;

            int[] codewordNumPlus;
            int codewordNumCounterValue;

            switch (QRCodeEncodeMode)
            {
                case MODE.ALPHA_NUMERIC:

                    codewordNumPlus = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4 };

                    dataValue[dataCounter] = 2;
                    dataCounter++;
                    dataValue[dataCounter] = dataLength;
                    dataBits[dataCounter] = 9;
                    codewordNumCounterValue = dataCounter;

                    dataCounter++;
                    for (int i = 0; i < dataLength; i++)
                    {
                        var chr = qrcodeData[i];
                        sbyte chrValue = 0;
                        if (chr >= 48 && chr < 58)
                        {
                            chrValue = (sbyte)(chr - 48);
                        }
                        else
                        {
                            if (chr >= 65 && chr < 91)
                            {
                                chrValue = (sbyte)(chr - 55);
                            }
                            else
                            {
                                switch (chr)
                                {
                                    case 32:

                                        chrValue = 36; break;
                                    case 36:

                                        chrValue = 37;
                                        break;
                                    case 37:

                                        chrValue = 38;
                                        break;
                                    case 42:

                                        chrValue = 39;
                                        break;
                                    case 43:

                                        chrValue = 40;
                                        break;
                                    case 45:

                                        chrValue = 41;
                                        break;
                                    case 46:

                                        chrValue = 42;
                                        break;
                                    case 47:

                                        chrValue = 43;
                                        break;
                                    case 58:

                                        chrValue = 44;
                                        break;
                                }
                            }
                        }
                        if ((i % 2) == 0)
                        {
                            dataValue[dataCounter] = chrValue;
                            dataBits[dataCounter] = 6;
                        }
                        else
                        {
                            dataValue[dataCounter] = dataValue[dataCounter] * 45 + chrValue;
                            dataBits[dataCounter] = 11;
                            if (i < dataLength - 1)
                            {
                                dataCounter++;
                            }
                        }
                    }
                    dataCounter++;
                    break;

                case MODE.NUMERIC:

                    codewordNumPlus = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4, 4 };

                    dataValue[dataCounter] = 1;
                    dataCounter++;
                    dataValue[dataCounter] = dataLength;

                    dataBits[dataCounter] = 10; /* #version 1-9*/
                    codewordNumCounterValue = dataCounter;

                    dataCounter++;
                    for (int i = 0; i < dataLength; i++)
                    {

                        if ((i % 3) == 0)
                        {
                            dataValue[dataCounter] = (int)(qrcodeData[i] - 0x30);
                            dataBits[dataCounter] = 4;
                        }
                        else
                        {

                            dataValue[dataCounter] = dataValue[dataCounter] * 10 + (int)(qrcodeData[i] - 0x30);

                            if ((i % 3) == 1)
                            {
                                dataBits[dataCounter] = 7;
                            }
                            else
                            {
                                dataBits[dataCounter] = 10;
                                if (i < dataLength - 1)
                                {
                                    dataCounter++;
                                }
                            }
                        }
                    }
                    dataCounter++;
                    break;

                default:/*8byte*/

                    codewordNumPlus = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8, 8 };
                    dataValue[dataCounter] = 4;
                    dataCounter++;
                    dataValue[dataCounter] = dataLength;
                    dataBits[dataCounter] = 8;
                    codewordNumCounterValue = dataCounter;

                    dataCounter++;

                    for (int i = 0; i < dataLength; i++)
                    {
                        dataValue[i + dataCounter] = (qrcodeData[i] & 0xFF);
                        dataBits[i + dataCounter] = 8;
                    }
                    dataCounter += dataLength;

                    break;

            }

            int totalDataBits = 0;
            for (int i = 0; i < dataCounter; i++)
            {
                totalDataBits += dataBits[i];
            }

            int ec;
            switch (QRCodeErrorCorrect)
            {

                case ERRORCORRECTION.L:
                    ec = 1;
                    break;

                case ERRORCORRECTION.Q:
                    ec = 3;
                    break;

                case ERRORCORRECTION.H:
                    ec = 2;
                    break;

                default:
                    ec = 0;
                    break;

            }


            var maxDataBitsArray = new int[][] { new int[] { 0, 128, 224, 352, 512, 688, 864, 992, 1232, 1456, 1728, 2032, 2320, 2672, 2920, 3320, 3624, 4056, 4504, 5016, 5352, 5712, 6256, 6880, 7312, 8000, 8496, 9024, 9544, 10136, 10984, 11640, 12328, 13048, 13800, 14496, 15312, 15936, 16816, 17728, 18672 }, new int[] { 0, 152, 272, 440, 640, 864, 1088, 1248, 1552, 1856, 2192, 2592, 2960, 3424, 3688, 4184, 4712, 5176, 5768, 6360, 6888, 7456, 8048, 8752, 9392, 10208, 10960, 11744, 12248, 13048, 13880, 14744, 15640, 16568, 17528, 18448, 19472, 20528, 21616, 22496, 23648 }, new int[] { 0, 72, 128, 208, 288, 368, 480, 528, 688, 800, 976, 1120, 1264, 1440, 1576, 1784, 2024, 2264, 2504, 2728, 3080, 3248, 3536, 3712, 4112, 4304, 4768, 5024, 5288, 5608, 5960, 6344, 6760, 7208, 7688, 7888, 8432, 8768, 9136, 9776, 10208 }, new int[] { 0, 104, 176, 272, 384, 496, 608, 704, 880, 1056, 1232, 1440, 1648, 1952, 2088, 2360, 2600, 2936, 3176, 3560, 3880, 4096, 4544, 4912, 5312, 5744, 6032, 6464, 6968, 7288, 7880, 8264, 8920, 9368, 9848, 10288, 10832, 11408, 12016, 12656, 13328 } };

            int maxDataBits = 0;

            if (QRCodeVersion == 0)
            {
                QRCodeVersion = 1;
                for (int i = 1; i <= _maxQrCodeVersionAvailable; i += _qrCodeVersionStep)
                {
                    if ((maxDataBitsArray[ec][i]) >= totalDataBits + codewordNumPlus[QRCodeVersion])
                    {
                        maxDataBits = maxDataBitsArray[ec][i];
                        break;
                    }
                    QRCodeVersion += _qrCodeVersionStep;
                }
            }
            else
            {
                maxDataBits = maxDataBitsArray[ec][QRCodeVersion];
            }

            totalDataBits += codewordNumPlus[QRCodeVersion];
            dataBits[codewordNumCounterValue] = (sbyte)(dataBits[codewordNumCounterValue] + codewordNumPlus[QRCodeVersion]);

            int[] maxCodewordsArray = new int[] { 0, 26, 44, 70, 100, 134, 172, 196, 242, 292, 346, 404, 466, 532, 581, 655, 733, 815, 901, 991, 1085, 1156, 1258, 1364, 1474, 1588, 1706, 1828, 1921, 2051, 2185, 2323, 2465, 2611, 2761, 2876, 3034, 3196, 3362, 3532, 3706 };

            int maxCodewords = maxCodewordsArray[QRCodeVersion];
            int maxModules1side = 17 + (QRCodeVersion << 2);

            int[] matrixRemainBit = new int[] { 0, 0, 7, 7, 7, 7, 7, 0, 0, 0, 0, 0, 0, 0, 3, 3, 3, 3, 3, 3, 3, 4, 4, 4, 4, 4, 4, 4, 3, 3, 3, 3, 3, 3, 3, 0, 0, 0, 0, 0, 0 };

            /*read version ECC data file*/

            int byte_num = matrixRemainBit[QRCodeVersion] + (maxCodewords << 3);

            sbyte[] matrixX = new sbyte[byte_num];
            sbyte[] matrixY = new sbyte[byte_num];
            sbyte[] maskArray = new sbyte[byte_num];
            sbyte[] formatInformationX2 = new sbyte[15];
            sbyte[] formatInformationY2 = new sbyte[15];
            sbyte[] rsEccCodewords = new sbyte[1];
            sbyte[] rsBlockOrderTemp = new sbyte[128];

            try
            {
                String fileName = "qrv" + Convert.ToString(QRCodeVersion) + "_" + Convert.ToString(ec);
                Stream stream = GetResourceFile(fileName);

                Read(stream, matrixX, 0, matrixX.Length);
                Read(stream, matrixY, 0, matrixY.Length);
                Read(stream, maskArray, 0, maskArray.Length);
                Read(stream, formatInformationX2, 0, formatInformationX2.Length);
                Read(stream, formatInformationY2, 0, formatInformationY2.Length);
                Read(stream, rsEccCodewords, 0, rsEccCodewords.Length);
                Read(stream, rsBlockOrderTemp, 0, rsBlockOrderTemp.Length);

                stream.Close();
            }
            catch (Exception ex)
            {
            }

            sbyte rsBlockOrderLength = 1;
            for (sbyte i = 1; i < 128; i++)
            {
                if (rsBlockOrderTemp[i] == 0)
                {
                    rsBlockOrderLength = i;
                    break;
                }
            }
            sbyte[] rsBlockOrder = new sbyte[rsBlockOrderLength];
            Array.Copy(rsBlockOrderTemp, 0, rsBlockOrder, 0, (byte)rsBlockOrderLength);


            sbyte[] formatInformationX1 = new sbyte[] { 0, 1, 2, 3, 4, 5, 7, 8, 8, 8, 8, 8, 8, 8, 8 };
            sbyte[] formatInformationY1 = new sbyte[] { 8, 8, 8, 8, 8, 8, 8, 8, 7, 5, 4, 3, 2, 1, 0 };

            int maxDataCodewords = maxDataBits >> 3;

            if(dataLength >= maxDataCodewords - 1)
            {
                return new bool[][] { new bool[1] };
            }

            /* read frame data  */

            int modules1Side = 4 * QRCodeVersion + 17;
            int matrixTotalBits = modules1Side * modules1Side;
            sbyte[] frameData = new sbyte[matrixTotalBits + modules1Side];

            try
            {
                Stream stream = GetResourceFile("qrvfr" + Convert.ToString(QRCodeVersion));
                Read(stream, frameData, 0, frameData.Length);
                stream.Close();
            }
            catch
            {
            }

            /*set terminator */

            if (totalDataBits <= maxDataBits - 4)
            {
                dataValue[dataCounter] = 0;
                dataBits[dataCounter] = 4;
            }
            else
            {
                if (totalDataBits < maxDataBits)
                {
                    dataValue[dataCounter] = 0;
                    dataBits[dataCounter] = (sbyte)(maxDataBits - totalDataBits);
                }
                else
                {
                    if (totalDataBits > maxDataBits)
                    {
                        System.Console.Out.WriteLine("overflow");
                    }
                }
            }
            sbyte[] dataCodewords = DivideDataBy8Bits(dataValue, dataBits, maxDataCodewords);
            sbyte[] codewords = CalculateRSECC(dataCodewords, rsEccCodewords[0], rsBlockOrder, maxDataCodewords, maxCodewords);

            /* flash matrix */

            sbyte[][] matrixContent = new sbyte[modules1Side][];
            for (int i2 = 0; i2 < modules1Side; i2++)
            {
                matrixContent[i2] = new sbyte[modules1Side];
            }

            for (int i = 0; i < modules1Side; i++)
            {
                for (int j = 0; j < modules1Side; j++)
                {
                    matrixContent[j][i] = 0;
                }
            }

            /* attach data */
            for (int i = 0; i < maxCodewords; i++)
            {

                sbyte codeword_i = codewords[i];
                for (int j = 7; j >= 0; j--)
                {

                    int codewordBitsNumber = (i * 8) + j;

                    matrixContent[matrixX[codewordBitsNumber] & 0xFF][matrixY[codewordBitsNumber] & 0xFF] = (sbyte)((255 * (codeword_i & 1)) ^ maskArray[codewordBitsNumber]);

                    codeword_i = (sbyte)(URShift((codeword_i & 0xFF), 1));
                }
            }

            for (int matrixRemain = matrixRemainBit[QRCodeVersion]; matrixRemain > 0; matrixRemain--)
            {
                int remainBitTemp = matrixRemain + (maxCodewords * 8) - 1;
                matrixContent[matrixX[remainBitTemp] & 0xFF][matrixY[remainBitTemp] & 0xFF] = (sbyte)(255 ^ maskArray[remainBitTemp]);
            }

            /* mask select*/
            sbyte maskNumber = SelectMask(matrixContent, matrixRemainBit[QRCodeVersion] + maxCodewords * 8);
            sbyte maskContent = (sbyte)(1 << maskNumber);

            /* format information */

            sbyte formatInformationValue = (sbyte)(ec << 3 | maskNumber);

            String[] formatInformationArray = new String[] { "101010000010010", "101000100100101", "101111001111100", "101101101001011", "100010111111001", "100000011001110", "100111110010111", "100101010100000", "111011111000100", "111001011110011", "111110110101010", "111100010011101", "110011000101111", "110001100011000", "110110001000001", "110100101110110", "001011010001001", "001001110111110", "001110011100111", "001100111010000", "000011101100010", "000001001010101", "000110100001100", "000100000111011", "011010101011111", "011000001101000", "011111100110001", "011101000000110", "010010010110100", "010000110000011", "010111011011010", "010101111101101" };

            for (int i = 0; i < 15; i++)
            {

                sbyte content = (sbyte)System.SByte.Parse(formatInformationArray[formatInformationValue].Substring(i, (i + 1) - (i)));

                matrixContent[formatInformationX1[i] & 0xFF][formatInformationY1[i] & 0xFF] = (sbyte)(content * 255);
                matrixContent[formatInformationX2[i] & 0xFF][formatInformationY2[i] & 0xFF] = (sbyte)(content * 255);
            }

            bool[][] out_Renamed = new bool[modules1Side][];
            for (int i3 = 0; i3 < modules1Side; i3++)
            {
                out_Renamed[i3] = new bool[modules1Side];
            }

            int c = 0;
            for (int i = 0; i < modules1Side; i++)
            {
                for (int j = 0; j < modules1Side; j++)
                {

                    if ((matrixContent[j][i] & maskContent) != 0 || frameData[c] == (char)49)
                    {
                        out_Renamed[j][i] = true;
                    }
                    else
                    {
                        out_Renamed[j][i] = false;
                    }
                    c++;
                }
                c++;
            }

            return out_Renamed;
        }

        sbyte[] DivideDataBy8Bits(int[] data, sbyte[] bits, int maxDataCodewords)
        {
            int l1 = bits.Length;
            int l2;
            int codewordsCounter = 0;
            int remainingBits = 8;
            int max = 0;
            int buffer;
            int bufferBits;
            bool flag;

            if (l1 != data.Length)
            {
            }
            for (int i = 0; i < l1; i++)
            {
                max += bits[i];
            }
            l2 = (max - 1) / 8 + 1;
            sbyte[] codewords = new sbyte[maxDataCodewords];
            for (int i = 0; i < l2; i++)
            {
                codewords[i] = 0;
            }
            for (int i = 0; i < l1; i++)
            {
                buffer = data[i];
                bufferBits = bits[i];
                flag = true;

                if (bufferBits == 0)
                {
                    break;
                }
                while (flag)
                {
                    if (remainingBits > bufferBits)
                    {
                        codewords[codewordsCounter] = (sbyte)((codewords[codewordsCounter] << bufferBits) | buffer);
                        remainingBits -= bufferBits;
                        flag = false;
                    }
                    else
                    {
                        bufferBits -= remainingBits;
                        codewords[codewordsCounter] = (sbyte)((codewords[codewordsCounter] << remainingBits) | (buffer >> bufferBits));

                        if (bufferBits == 0)
                        {
                            flag = false;
                        }
                        else
                        {
                            buffer = (buffer & ((1 << bufferBits) - 1));
                            flag = true;
                        }
                        codewordsCounter++;
                        remainingBits = 8;
                    }
                }
            }
            if (remainingBits != 8)
            {
                codewords[codewordsCounter] = (sbyte)(codewords[codewordsCounter] << remainingBits);
            }
            else
            {
                codewordsCounter--;
            }
            if (codewordsCounter < maxDataCodewords - 1)
            {
                flag = true;
                while (codewordsCounter < maxDataCodewords - 1)
                {
                    codewordsCounter++;
                    if (flag)
                    {
                        codewords[codewordsCounter] = -20;
                    }
                    else
                    {
                        codewords[codewordsCounter] = 17;
                    }
                    flag = !(flag);
                }
            }
            return codewords;
        }


        sbyte[] CalculateRSECC(sbyte[] codewords, sbyte rsEccCodewords, sbyte[] rsBlockOrder, int maxDataCodewords, int maxCodewords)
        {

            sbyte[][] rsCalTableArray = new sbyte[256][];
            for (int i = 0; i < 256; i++)
            {
                rsCalTableArray[i] = new sbyte[rsEccCodewords];
            }

            String fileName = "rsc" + rsEccCodewords.ToString();
            Stream stream = GetResourceFile(fileName);
            if (stream == null)
                throw new IOException("can't load rsc file");
            for (int i = 0; i < 256; i++)
            {
                Read(stream, rsCalTableArray[i], 0, rsCalTableArray[i].Length);
            }
            stream.Close();



            /* RS-ECC prepare */

            int i2 = 0;
            int j = 0;
            int rsBlockNumber = 0;

            sbyte[][] rsTemp = new sbyte[rsBlockOrder.Length][];
            sbyte[] res = new sbyte[maxCodewords];
            Array.Copy(codewords, 0, res, 0, codewords.Length);

            i2 = 0;
            while (i2 < rsBlockOrder.Length)
            {
                rsTemp[i2] = new sbyte[(rsBlockOrder[i2] & 0xFF) - rsEccCodewords];
                i2++;
            }
            i2 = 0;
            while (i2 < maxDataCodewords)
            {
                rsTemp[rsBlockNumber][j] = codewords[i2];
                j++;
                if (j >= (rsBlockOrder[rsBlockNumber] & 0xFF) - rsEccCodewords)
                {
                    j = 0;
                    rsBlockNumber++;
                }
                i2++;
            }

            /* RS-ECC main */

            rsBlockNumber = 0;
            while (rsBlockNumber < rsBlockOrder.Length)
            {
                sbyte[] rsTempData;
                rsTempData = new sbyte[rsTemp[rsBlockNumber].Length];
                rsTemp[rsBlockNumber].CopyTo(rsTempData, 0);

                int rsCodewords = (rsBlockOrder[rsBlockNumber] & 0xFF);
                int rsDataCodewords = rsCodewords - rsEccCodewords;

                j = rsDataCodewords;
                while (j > 0)
                {
                    sbyte first = rsTempData[0];
                    if (first != 0)
                    {
                        sbyte[] leftChr = new sbyte[rsTempData.Length - 1];
                        Array.Copy(rsTempData, 1, leftChr, 0, rsTempData.Length - 1);
                        sbyte[] cal = rsCalTableArray[(first & 0xFF)];
                        rsTempData = CalculateByteArrayBits(leftChr, cal, "xor");
                    }
                    else
                    {
                        if (rsEccCodewords < rsTempData.Length)
                        {
                            sbyte[] rsTempNew = new sbyte[rsTempData.Length - 1];
                            Array.Copy(rsTempData, 1, rsTempNew, 0, rsTempData.Length - 1);
                            rsTempData = new sbyte[rsTempNew.Length];
                            rsTempNew.CopyTo(rsTempData, 0);
                        }
                        else
                        {
                            sbyte[] rsTempNew = new sbyte[rsEccCodewords];
                            Array.Copy(rsTempData, 1, rsTempNew, 0, rsTempData.Length - 1);
                            rsTempNew[rsEccCodewords - 1] = 0;
                            rsTempData = new sbyte[rsTempNew.Length];
                            rsTempNew.CopyTo(rsTempData, 0);
                        }
                    }
                    j--;
                }

                Array.Copy(rsTempData, 0, res, codewords.Length + rsBlockNumber * rsEccCodewords, (byte)rsEccCodewords);
                rsBlockNumber++;
            }
            return res;
        }

        sbyte[] CalculateByteArrayBits(sbyte[] xa, sbyte[] xb, String ind)
        {
            int ll;
            int ls;
            sbyte[] res;
            sbyte[] xl;
            sbyte[] xs;

            if (xa.Length > xb.Length)
            {
                xl = new sbyte[xa.Length];
                xa.CopyTo(xl, 0);
                xs = new sbyte[xb.Length];
                xb.CopyTo(xs, 0);
            }
            else
            {
                xl = new sbyte[xb.Length];
                xb.CopyTo(xl, 0);
                xs = new sbyte[xa.Length];
                xa.CopyTo(xs, 0);
            }
            ll = xl.Length;
            ls = xs.Length;
            res = new sbyte[ll];

            for (int i = 0; i < ll; i++)
            {
                if (i < ls)
                {
                    if ((System.Object)ind == (System.Object)"xor")
                    {
                        res[i] = (sbyte)(xl[i] ^ xs[i]);
                    }
                    else
                    {
                        res[i] = (sbyte)(xl[i] | xs[i]);
                    }
                }
                else
                {
                    res[i] = xl[i];
                }
            }
            return res;
        }

        sbyte SelectMask(sbyte[][] matrixContent, int maxCodewordsBitWithRemain)
        {
            int l = matrixContent.Length;
            var d1 = new int[8];
            var d2 = new int[8];
            var d3 = new int[8];
            var d4 = new int[8];

            int d2And = 0;
            int d2Or = 0;
            var d4Counter = new int[8];

            for (int y = 0; y < l; y++)
            {
                var xData = new int[8];
                var yData = new int[8];
                var xD1Flag = new bool[8];
                var yD1Flag = new bool[8];

                for (int x = 0; x < l; x++)
                {

                    if (x > 0 && y > 0)
                    {
                        d2And = matrixContent[x][y] & matrixContent[x - 1][y] & matrixContent[x][y - 1] & matrixContent[x - 1][y - 1] & 0xFF;
                        d2Or = (matrixContent[x][y] & 0xFF) | (matrixContent[x - 1][y] & 0xFF) | (matrixContent[x][y - 1] & 0xFF) | (matrixContent[x - 1][y - 1] & 0xFF);
                    }

                    for (int maskNumber = 0; maskNumber < 8; maskNumber++)
                    {

                        xData[maskNumber] = ((xData[maskNumber] & 63) << 1) | ((URShift((matrixContent[x][y] & 0xFF), maskNumber)) & 1);
                        yData[maskNumber] = ((yData[maskNumber] & 63) << 1) | ((URShift((matrixContent[y][x] & 0xFF), maskNumber)) & 1);


                        if ((matrixContent[x][y] & (1 << maskNumber)) != 0)
                            d4Counter[maskNumber]++;

                        if (xData[maskNumber] == 93)
                            d3[maskNumber] += 40;

                        if (yData[maskNumber] == 93)
                            d3[maskNumber] += 40;

                        if (x > 0 && y > 0)
                        {

                            if (((d2And & 1) != 0) || ((d2Or & 1) == 0))
                            {
                                d2[maskNumber] += 3;
                            }

                            d2And = d2And >> 1;
                            d2Or = d2Or >> 1;
                        }

                        if (((xData[maskNumber] & 0x1F) == 0) || ((xData[maskNumber] & 0x1F) == 0x1F))
                        {
                            if (x > 3)
                            {
                                if (xD1Flag[maskNumber])
                                {
                                    d1[maskNumber]++;
                                }
                                else
                                {
                                    d1[maskNumber] += 3;
                                    xD1Flag[maskNumber] = true;
                                }
                            }
                        }
                        else
                        {
                            xD1Flag[maskNumber] = false;
                        }
                        if (((yData[maskNumber] & 0x1F) == 0) || ((yData[maskNumber] & 0x1F) == 0x1F))
                        {
                            if (x > 3)
                            {
                                if (yD1Flag[maskNumber])
                                {
                                    d1[maskNumber]++;
                                }
                                else
                                {
                                    d1[maskNumber] += 3;
                                    yD1Flag[maskNumber] = true;
                                }
                            }
                        }
                        else
                        {
                            yD1Flag[maskNumber] = false;
                        }
                    }
                }
            }
            int minValue = 0;
            sbyte res = 0;
            var d4Value = new int[] { 90, 80, 70, 60, 50, 40, 30, 20, 10, 0, 0, 10, 20, 30, 40, 50, 60, 70, 80, 90, 90 };
            for (int maskNumber = 0; maskNumber < 8; maskNumber++)
            {
                d4[maskNumber] = d4Value[(int)((20 * d4Counter[maskNumber]) / maxCodewordsBitWithRemain)];
                int demerit = d1[maskNumber] + d2[maskNumber] + d3[maskNumber] + d4[maskNumber];
                if (demerit < minValue || maskNumber == 0)
                {
                    res = (sbyte)maskNumber;
                    minValue = demerit;
                }
            }
            return res;
        }

        int Read(System.IO.Stream sourceStream, sbyte[] target, int start, int count)
        {
            if (target.Length == 0)
                return 0;

            byte[] receiver = new byte[target.Length];
            int bytesRead = sourceStream.Read(receiver, start, count);

            if (bytesRead == 0)
                return -1;

            for (int i = start; i < start + bytesRead; i++)
                target[i] = (sbyte)receiver[i];

            return bytesRead;
        }

        /// <summary>
        /// Performs an unsigned bitwise right shift with the specified number
        /// </summary>
        /// <param name="number">Number to operate on</param>
        /// <param name="bits">Ammount of bits to shift</param>
        /// <returns>The resulting number from the shift operation</returns>
        int URShift(int number, int bits)
        {
            if (number >= 0)
                return number >> bits;
            else
                return (number >> bits) + (2 << ~bits);
        }
        #endregion

        public byte[] Generate(string content, Encoding encoding)
        {
            bool[][] matrix = CalQrcode(encoding.GetBytes(content));
            byte[] returnBuffer = new byte[0];

            if (matrix.Length > 1)
            {
                Image img = new Bitmap(
                    (matrix.Length) * QRCodeScale,
                    (matrix.Length) * QRCodeScale,
                    PixelFormat.Format32bppArgb);

                Graphics gfx = Graphics.FromImage(img);
                gfx.Clear(QRCodeBackgroundColor);

                var color = QRCodeForegroundColor;
                var a = color.A + 1;
                var col = (color.A << 24)
                   | ((byte)((color.R * a) >> 8) << 16)
                   | ((byte)((color.G * a) >> 8) << 8)
                   | ((byte)((color.B * a) >> 8));

                for (int y = 0; y < matrix.Length; y++)
                {
                    for (int x = 0; x < matrix.Length; x++)
                    {
                        if (matrix[x][y])
                        {
                            //gfx.DrawRectangle(new Pen(
                            //    new SolidBrush(
                            //        Color.FromArgb(col)), 
                            //        0.1f), 
                            //    x * QRCodeScale, 
                            //    y * QRCodeScale, 
                            //    QRCodeScale, 
                            //    QRCodeScale
                            //);

                            gfx.FillRectangle(new SolidBrush(
                                    Color.FromArgb(col)),
                                    x * QRCodeScale,
                                    y * QRCodeScale,
                                    QRCodeScale,
                                    QRCodeScale
                            );
                        }
                    }
                }

                MemoryStream ms = new MemoryStream();
                img.Save(ms, ImageFormat.Png);
                returnBuffer = ms.GetBuffer();

                gfx.Dispose();
            }

            //return (Bitmap)img;

            return returnBuffer;
        }

        public string GenerateQRCodeString(string content, Encoding encoding)
        {
            bool[][] matrix = CalQrcode(encoding.GetBytes(content));
            string qrcodeinstring;

            // debug matrix content. 
            {
                string m = "";

                for (int i = 0; i < matrix.Length; i++)
                {
                    for (int j = 0; j < matrix.Length; j++)
                    {
                        m += matrix[j][i] ? "█" : "░";
                    }
                    m += "\n";
                }

                qrcodeinstring = m;
            }

            return qrcodeinstring;
        }

        public byte[] Generate(String content)
        {
            return Generate(content, Encoding.UTF8);
        }

        internal Stream GetResourceFile(string fileName)
        {
            var res = NETStandard.JaraQRCode.ResourceManager.GetObject(fileName);

            return new MemoryStream((byte[])res);
        }
    }
}