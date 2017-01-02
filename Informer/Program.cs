using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Mail;
using System.Xml;
using System.Xml.Linq;
using System.Data;
using System.Drawing;
using System.IO;

namespace PiramidaInformer
{
    class Program
    {
        static void Main(string[] args)
        {
            string path = Directory.GetCurrentDirectory();
            DateTime yesterday = DateTime.Today.AddDays(-1).Date;
            Console.WriteLine("НЕ ЗАКРЫВАЙТЕ ЭТО ОКНО!!!");
            Console.WriteLine();
            string okMessage;
            DataProvider d;
            Settings settings;
            Font fontTickLabels = new Font("Arial", 10);
            try
            {
                settings = new Settings("Settings.ini");
                d = new DataProvider(settings.Server, settings.Database, settings.UserName, settings.Password);
            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
                return;
            }
            if (settings.CleanUp == 1)
            {
                foreach (string file in Directory.GetFiles(path))
                {
                    if (Path.GetExtension(file) == ".jpg")
                        File.Delete(file);
                }
            }
            MailMessage msg = new MailMessage(settings.AddressFrom, settings.AddressTo,
                "Оперативная информация из Пирамиды2000 за " + yesterday.ToShortDateString(),
                "В сообщение включены следующие объекты:\n\n");
            XDocument doc = XDocument.Load("InformerTasks.xml");

            // Построение карты сбора
            XElement map = doc.Descendants("Map").First();
            var substations = map.Descendants("Substation");
            Bitmap mapPic = new Bitmap(1280, 720);
            Graphics mapGraphic = Graphics.FromImage(mapPic);
            Pen blackPen = new Pen(Color.Black, 1);
            Pen gridPen = new Pen(Color.Black, 1);
            mapGraphic.FillRegion(Brushes.White, new Region(mapGraphic.ClipBounds));
            mapGraphic.DrawString(string.Format("Карта сбора за {0}", yesterday.ToShortDateString()),
                                  new Font("Arial", 16), Brushes.Black, 400, 10);
            int rowPosition = 0;
            int colPosition = 0;
            int rowHeight = 40;
            int rowOffset = 60;
            int colWidth = 250;
            int colOffset = 20;
            foreach (XElement substation in substations)
            {
                int objCode = int.Parse(substation.Attributes("objectCode").First().Value);
                string objName = substation.Attributes("name").First().Value;
                var items = substation.Descendants("Meter");
                int total = items.Count();
                int gathered = 0;
                foreach (XElement item in items)
                {
                    gathered += d.GatheredData(objCode,
                        int.Parse(item.Attributes("itemCode").First().Value),
                        yesterday);
                }
                mapGraphic.DrawString(objName, new Font("Arial", 12), Brushes.Black,
                    colPosition * colWidth + colOffset, rowPosition * rowHeight + rowOffset);
                Rectangle cell = new Rectangle(colPosition * colWidth + colWidth + colOffset - 40,
                                               rowPosition * rowHeight + rowOffset - rowHeight / 3,
                                               40, rowHeight);
                mapGraphic.FillRectangle(SelectColor(100f * gathered / total / 48), cell);
                cell = new Rectangle(colPosition * colWidth + colOffset,
                                     rowPosition * rowHeight + rowOffset - rowHeight / 3,
                                                               colWidth, rowHeight);
                mapGraphic.DrawRectangle(blackPen, cell);
                rowPosition++;
                if (rowPosition > 14)
                {
                    rowPosition = 0;
                    colPosition++;
                }
            }
            string fileName = Path.Combine(path, "Карта_сбора_за_" + yesterday.ToString("dd-MM-yyyy") + ".jpg");
            mapPic.Save(fileName);
            mapGraphic.Dispose();
            mapPic.Dispose();
            // Attach the map to e-mail message
            msg.Attachments.Add(new Attachment(fileName));
            msg.Body = msg.Body + "Карта сбора\n";
            Console.WriteLine("Выполнено: карта сбора");

            // Построение графиков
            var tasks = doc.Descendants("Task");
            foreach (XElement task in tasks)
            {
                string picTitle = task.Descendants("Title").First().Value;
                int objCode = int.Parse(task.Descendants("ObjectCode").First().Value);
                var items = task.Descendants("Item");
                Bitmap pic = new Bitmap(1280, 720);
                Graphics c = Graphics.FromImage(pic);
                c.FillRegion(Brushes.White, new Region(c.ClipBounds));
                gridPen.DashStyle = System.Drawing.Drawing2D.DashStyle.Dash;
                c.DrawRectangle(blackPen, 60, 60, 940, 600);
                c.DrawString(string.Format("ПС {0} за {1}", picTitle, yesterday.ToShortDateString()),
                             new Font("Arial", 16), Brushes.Black, 400, 10);
                #region Get profiles
                List<PlotLine> data = new List<PlotLine>();
                foreach (XElement item in items)
                {
                    int itemCode = int.Parse(item.Descendants("ItemCode").First().Value);
                    string color = item.Descendants("Color").First().Value;
                    string plotTitle = d.GetItemName(objCode, itemCode);
                    DataTable profile = new DataTable();
                    try
                    {
                        profile = d.GetProfile(objCode, itemCode, yesterday);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log(ex.Message);
                        return;
                    }
                    PointF[] points = new PointF[48];
                    string[] labels = new string[48];
                    int i = 0;
                    foreach (DataRow row in profile.Rows)
                    {
                        points[i].X = 60 + i * 20;
                        if (Convert.IsDBNull(row[3]))
                            points[i].Y = 0;
                        else
                            points[i].Y = float.Parse(row[3].ToString());
                        labels[i] = row[2].ToString();
                        i++;
                    }
                    data.Add(new PlotLine(points, labels, color, plotTitle));
                }
                #endregion

                #region Find scale factor

                float maxPower = data.Max((PlotLine pl) => pl.MaxPoint);
                float scale;
                float magnitude;
                if (maxPower == 0)
                    scale = 1;
                else
                {
                    if (maxPower > 600)
                    {
                        scale = maxPower / 600;
                        magnitude = (float)Math.Pow(10, Math.Floor(Math.Log10(scale)));
                        scale = (float)Math.Ceiling(scale / magnitude) * magnitude;
                    }
                    else
                    {
                        scale = 1;
                    }
                }
                foreach (PlotLine plot in data)
                {
                    for (int i = 0; i < 48; i++)
                        plot.Points[i].Y = 660 - plot.Points[i].Y / scale;
                }
                #endregion

                #region Draw grid
                for (int x = 1; x <= 47; x++)
                {
                    c.DrawLine(gridPen, 60 + 20 * x, 60, 60 + 20 * x, 660);
                    if (x % 6 == 5)
                    {
                        c.DrawLine(gridPen, 60 + 20 * x, 660, 60 + 20 * x, 665);
                        c.DrawString(data[0].Labels[x],
                            fontTickLabels, Brushes.Black, 40 + 20 * x, 670);
                    }
                }
                for (int y = 1; y <= 5; y++)
                {
                    c.DrawLine(gridPen, 50, 60 + 100 * y, 1000, 60 + 100 * y);
                    c.DrawString((100 * (6 - y) * scale).ToString(),
                        fontTickLabels, Brushes.Black, 10, 55 + y * 100);
                }
                #endregion

                #region Actually draw plots

                // Draw legend
                int plotNumber = 1;
                try
                {
                    foreach (PlotLine pl in data)
                    {
                        c.DrawLines(pl.PlotPen, pl.Points);
                        c.FillRectangle(pl.PlotPen.Brush, 1010, 30 + 30 * plotNumber, 30, 15);
                        c.DrawString(pl.Title, fontTickLabels, Brushes.Black, 1050, 30 + 30 * plotNumber);
                        plotNumber++;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(ex.Message);
                    return;
                }
                #endregion

                fileName = Path.Combine(path, picTitle + "-" + yesterday.ToString("dd-MM-yyyy") + ".jpg");
                pic.Save(fileName);
                c.Dispose();
                pic.Dispose();
                // Attach pictures to e-mail message
                msg.Attachments.Add(new Attachment(fileName));
                msg.Body = msg.Body + picTitle + "\n";
                Console.WriteLine("Выполнено: " + picTitle);
            }

            Console.WriteLine();
            Console.WriteLine("Ждите, выполняется отправка сообщения...");
            try
            {
                SmtpClient smtp = new SmtpClient(settings.SMTPServer, settings.SMTPPort);
                if (settings.UseSSL == 1)
                    smtp.EnableSsl = true;
                smtp.UseDefaultCredentials = false;
                smtp.Credentials = new System.Net.NetworkCredential(settings.SMTPUserName, settings.SMTPPassword);
                smtp.Send(msg);

            }
            catch (Exception ex)
            {
                Logger.Log(ex.Message);
                return;
            }

            Console.WriteLine("Сообщение отправлено");
            okMessage = DateTime.Now.ToString() + ": Завершено успешно";
            Logger.Log(okMessage);
        }

        private static Brush SelectColor(float percent)
        {
            Brush result;
            if (percent == 0)
                result = Brushes.Red;
            else if (percent < 50)
                result = Brushes.Orange;
            else if (percent < 90)
                result = Brushes.Yellow;
			else if (percent < 100)
				result = Brushes.Lime;
            else if (percent == 100)
                result = Brushes.Green;
            else
                result = Brushes.Black;
            return result;
        }
    }


}
