using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Web;
using System.Net;
using System.Diagnostics;
using System.Net.Http;
using HtmlAgilityPack;
using System.Collections;

namespace Hearthstone_Combos
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public List<card> CardDB = new List<card>();//List of all cards for selection
        public List<Image> combo_image = new List<Image>();//List of all the images in the combo
        public int index = 0;//The integer that tracks the location of the card in the combo image
        public string highlight_name = "";//The name of the highlighted card
        public string[] combo = new string[4];//The array of card names that make up the combo
        public List<card> card_combo = new List<card>();//The list of cards that make up the combo
        public List<string> urlList = new List<string>();//List of image urls from the website
        public List<BitmapImage> ImageList = new List<BitmapImage>();//List of images of cards
        public List<decimal> final_probaility = new List<decimal>();//List of probabilities

        public MainWindow()
        {
            InitializeComponent();
            GetAllImages("http://www.hearthpwn.com/guides/1840-mean-streets-of-gadgetzan-hearthstones-fourth");//Get the image links from the website
            Set_Cards();//Populate the CardDB from the csv file
            Set_Combo_Images();//Create a list of Images for the combo cards
            Set_Lists();//Populate the list of cards
        }

        private void Set_Lists()
        {
            for (int i = 0; i < CardDB.Count; i++)
            {
                CardList.Items.Add(CardDB[i].name);//Add an item to the list of cards from CardDB
            }
        }

        private void searchbox_TextChanged(object sender, EventArgs e)
        {
            var itemList = CardList.Items.Cast<string>().ToList();//Create a list of all items
            CardList.Items.Clear();//Clear the list
            if (!string.IsNullOrEmpty(searchbox.Text))//Check if user has input a string
            {
                foreach (string str in itemList)
                {
                    if (str.Contains(searchbox.Text))//Check to see if each card contains the user input string
                    {
                        CardList.Items.Add(str);//Add the list of strings that match
                    }
                }
            }
            else
            {
                Set_Lists();//Otherwise populate the string with the original set of cards
            }
        }

        private decimal binomial(decimal n, decimal k)
        {
            //Return a decimals that is the binomal of n and k (nCk)
            decimal result = 1;
            for (int i = 1; i <= k; i++)
            {
                result *= n - (k - i);
                result /= i;
            }
            return result;
        }

        private List<decimal> Find_Probability(decimal copies)
        {
            //Set the parameters of the distribution
            decimal N = 30;
            decimal K = copies;
            decimal k = 1;
            List<decimal> probabilities = new List<decimal>();
            //Find the probability of drawing the card on each turn
            for (int i = 5; i < 11; i++)
            {
                if((binomial(K, k) * binomial(N - K, i - k)) / binomial(N, i) != 0)
                {
                    probabilities.Add((binomial(K, k) * binomial(N - K, i - k)) / binomial(N, i));
                }
                else
                {
                    probabilities.Add(1);
                }
            }
            return probabilities;
        }

        public void GetAllImages(string url)
        {
            var document = new HtmlWeb().Load(url);//Load the webpage at the input url
            var urls = document.DocumentNode.Descendants("img")
                                            .Select(e => e.GetAttributeValue("src", null))
                                            .Where(s => !String.IsNullOrEmpty(s));//Find all of the urls with img in the attribute 
            foreach(var k in urls)
            {
                urlList.Add(k);//Add all of the image urls to a list
            }
        }
        
        private void Set_Cards()
        {

            List <string> listA = new List<string>();//Create a list for the names of the cards in the csv file
            using (var fs = File.OpenRead(@"C:\Users\Eugene\Documents\gadgetzan.csv"))//Open the file
            using (var reader = new StreamReader(fs))
            {
                while (!reader.EndOfStream)
                {
                    //Add each line to the list
                    string line = reader.ReadLine();
                    listA.Add(line.Trim());
                }
            }

            for(int i = 1; i < listA.Count; i++)
            {
                CardDB.Add(new card { name = listA[i] });//Create the list of cards using the names from the file
            }

            if (File.Exists(@"C:\Users\Eugene\Pictures\Hearthstone Cards\Mistress of Mixture.png"))//Check to see if the user has the images
            {
                foreach (card c in CardDB)
                {
                    ImageList.Add(new BitmapImage(new Uri(@"C:\Users\Eugene\Pictures\Hearthstone Cards\" + c.name + ".png")));//Load from file
                }
            }
            else
            {
                WebClient w = new WebClient();//Open a web browser
                for (int i = 0; i < CardDB.Count; i++)
                {
                    //Download the images from the url and save to file
                    w.DownloadFile(urlList[i + 1], @"C:\Users\Eugene\Pictures\Hearthstone Cards\" + CardDB[i].name + ".png");
                }
                //Get the images from the urls and store them in an Imagelist
                foreach (string img in urlList)
                    {
                        WebRequest request = WebRequest.Create(img);
                        WebResponse resp = request.GetResponse();
                        Stream respStream = resp.GetResponseStream();
                        respStream.Dispose();
                        BitmapImage bitmap = new BitmapImage();
                        bitmap.BeginInit();
                        bitmap.UriSource = new Uri(img, UriKind.Absolute);
                        bitmap.EndInit();
                        ImageList.Add(bitmap);
                    }
            }
        }

        public void Set_Combo_Images()
        {
            //Setup the list of images
            combo_image.Add(img1);
            combo_image.Add(img2);
            combo_image.Add(img3);
            combo_image.Add(img4);
        }

        public class card
        {
            //Class that stores the information of a Hearthstone Card
            public string name { get; set; }
            public int power { get; set; }
            public int tough { get; set; }
            public int cost { get; set; }
            public string spell { get; set; }
            public Image image { get; set; }
        }

        private void S(object sender, SelectionChangedEventArgs e)
        {

        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            //Reset all of the images to card backs
            Selected.Source = new BitmapImage(new Uri(@"C:\Users\Eugene\Pictures\cardback.jpg"));
            img1.Source = new BitmapImage(new Uri(@"C:\Users\Eugene\Pictures\cardback.jpg"));
            img2.Source = new BitmapImage(new Uri(@"C:\Users\Eugene\Pictures\cardback.jpg"));
            img3.Source = new BitmapImage(new Uri(@"C:\Users\Eugene\Pictures\cardback.jpg"));
            img4.Source = new BitmapImage(new Uri(@"C:\Users\Eugene\Pictures\cardback.jpg"));
            index = 0;//Add cards from the start
            //Set all of the final probabilities to zero
            final_probaility.Clear();
            //Set all of the text to blank
            turn1.Text = null;
            turn2.Text = null;
            turn3.Text = null;
            turn4.Text = null;
            turn5.Text = null;
            turn6.Text = null;
        }

        private void CardList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(CardList.SelectedItem != null)//Check to see if the a selection has been made
            {
                highlight_name = CardList.SelectedItem.ToString();//Find the highlighted card
            }
            Selected.Source = new BitmapImage(new Uri(@"C:\Users\Eugene\Pictures\Hearthstone Cards\"+highlight_name+".png"));//Show the image of that card
        }

        private void CardList_MouseDoubleClick(object sender, EventArgs e)
        {
            //If you are at the end change the index to 0 to start over
            if (index == 4)
            {
                index = 0;
            }
            //Set the image of the indexed card
            combo_image[index].Source = new BitmapImage(new Uri(@"C:\Users\Eugene\Pictures\Hearthstone Cards\" + highlight_name + ".png"));
            //Find card that matches the highlighted
            foreach (var c in CardDB)
            {
                if (highlight_name == c.name)
                {
                    combo[index] = highlight_name;//set the name to the list of the combo
                }
            }
            index++;//Go to the next index
        }
        
        private void Selected_MouseDown(object sender, EventArgs e)
        {
            //Set the image of the indexed card
            combo_image[index].Source = new BitmapImage(new Uri(@"C:\Users\Eugene\Pictures\Hearthstone Cards\" + highlight_name + ".png"));
            //If you are at the end change the index to 0 to start over
            if (index == 3)
            {
                index = 0;
            }
            //Find card that matches the highlighted
            foreach(var c in CardDB)
            {
                if(highlight_name == c.name)
                {
                    combo[index] = highlight_name;//set the name to the list of the combo
                }
            }
            index++;//Go to the next index
        }

        private void probability_Click(object sender, RoutedEventArgs e)
        {
            //Clear Probability
            final_probaility.Clear();
            //Check the text boxes for the number of copies of the card
            decimal c1 = Convert.ToDecimal(combo1.Text);
            decimal c2 = Convert.ToDecimal(combo2.Text);
            decimal c3 = Convert.ToDecimal(combo3.Text);
            decimal c4 = Convert.ToDecimal(combo4.Text);
            //Find the probabilities for each card at each turn
            for (int i = 0; i < 6; i++)
            {
                final_probaility.Add(Math.Round(Find_Probability(c1)[i] * Find_Probability(c2)[i] * Find_Probability(c3)[i] * Find_Probability(c4)[i]*100,2));
            }
            //Set the textboxes to the correct probability
            turn1.Text = final_probaility[0].ToString()+"%";
            turn2.Text = final_probaility[1].ToString()+"%";
            turn3.Text = final_probaility[2].ToString()+"%";
            turn4.Text = final_probaility[3].ToString()+"%";
            turn5.Text = final_probaility[4].ToString() + "%";
            turn6.Text = final_probaility[5].ToString() + "%";
        }
    }
}
