using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace daff_pixel_sort
{
    public partial class SortForm : Form
    {
        //Image loaded
        private         Bitmap  loadedImage;
        //Cloned copy of loaded image, with specifid pixel format
        private         Bitmap  clonedImage;

        private static  Color   initialPixel    = Color.White;
        private static  Color   maxPixel        = Color.Black;

        //THRESHOLDS
        private         float   darkThreshold   = 0.20f;
        private         float   redThreshold    = 0.50f;
        private         float   greenThreshold  = 0.50f;
        private         float   blueThreshold   = 0.50f;

        //SORTING BY DARKNESS
        private         Color[] toSortDark;
        //sortArrFresh is true if the array has JUST been initialized
        private         bool    toSortDarkFresh    = false;
        //The position where our sorted array of dark bits will be reinserted at
        private         int     currDarkPos     = -1;
        //the current number of items added to our sorting array
        private         int     currDarkIndex   = -1;

        //SORTING BY REDNESS
        //WE REUSE THE DARK STUFF

        //SORTING BY RGB
        //R
        private float[] toSortRed;
        //sortArrFresh is true if the array has JUST been initialized
        private bool toSortRedFresh = false;
        //The position where our sorted array of dark bits will be reinserted at
        private int currRedPos = -1;
        //the current number of items added to our sorting array
        private int currRedIndex = -1;
        //G
        private float[] toSortGreen;
        //sortArrFresh is true if the array has JUST been initialized
        private bool toSortGreenFresh = false;
        //The position where our sorted array of dark bits will be reinserted at
        private int currGreenPos = -1;
        //the current number of items added to our sorting array
        private int currGreenIndex = -1;
        //B
        private float[] toSortBlue;
        //sortArrFresh is true if the array has JUST been initialized
        private bool toSortBlueFresh = false;
        //The position where our sorted array of dark bits will be reinserted at
        private int currBluePos = -1;
        //the current number of items added to our sorting array
        private int currBlueIndex = -1;



        public SortForm()
        {
            InitializeComponent();
            numericUpDown1.Increment        = (decimal)0.05;
            numericUpDown1.DecimalPlaces    = 2;
            pictureBox1.SizeMode = PictureBoxSizeMode.Normal;
        }
        //FORM ELEMENT CALLBACK FUNCTIONS
        private void btnShow_Click(object sender, EventArgs e)
        {
            //Show the open file dialog,
            //and if the user selects an image
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                //Store the loaded image in a cloned image variable to be manipulated
                loadedImage = new Bitmap(openFileDialog1.FileName);
                clonedImage = new Bitmap(loadedImage.Width, loadedImage.Height, System.Drawing.Imaging.PixelFormat.Format32bppPArgb);
                //clone the image 
                //using (Graphics gr = Graphics.FromImage(clonedImage))
                //{
                //    gr.DrawImage(loadedImage, new Rectangle(0, 0, clonedImage.Width, clonedImage.Height));
                //}
                //Load the image and display it in the picturebox
                pictureBox1.Load(openFileDialog1.FileName);
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            //Clear the picture box of any image
            pictureBox1.Image = null;
        }

        private void btnBGColour_Click(object sender, EventArgs e)
        {
            //Show the colour dialog box. 
            //If the user clicks OK, change the picturebox's background color
            //to the colour the user selected
            if (colorDialog1.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.BackColor = colorDialog1.Color;
                btnBGColour.BackColor = colorDialog1.Color;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            //close the form application
            this.Close();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            //if the checkbox is selected,
            if (checkBox1.Checked)
            {
                //change the PictureBox's SizeMode property to "Stretch"    
                pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
            }
            else
            {
                //otherwise, 
                //change it to "Normal"
                pictureBox1.SizeMode = PictureBoxSizeMode.Normal;
                
            }
        }


        private void btnSortDark_Click(object sender, EventArgs e)
        {
            SortTheDarkBits();
        }

        private void btnSortRed_Click(object sender, EventArgs e)
        {
            SortTheRedBits();
        }

        private void btnSortRGB_Click(object sender, EventArgs e)
        {
            SortTheColours();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            darkThreshold = (float)numericUpDown1.Value;
        }

        private void numericUpDownR_ValueChanged(object sender, EventArgs e)
        {
            redThreshold = (float)numericUpDownR.Value;
        }

        private void numericUpDownG_ValueChanged(object sender, EventArgs e)
        {
            greenThreshold = (float)numericUpDownG.Value;
        }

        private void numericUpDownB_ValueChanged(object sender, EventArgs e)
        {
            blueThreshold = (float)numericUpDownB.Value;
        }

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            //if the dialog is successful
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                ImageFormat saveFormat = ImageFormat.Png;
                switch (saveFileDialog1.FilterIndex)
                {
                    //JPG
                    case 0:
                        saveFormat = ImageFormat.Jpeg;
                        break;
                    //PNG
                    case 1:
                        saveFormat = ImageFormat.Png;
                        break;
                    //BITMAP
                    case 2:
                        saveFormat = ImageFormat.Bmp;
                        break;
                }
                //save the bitmap to a file
                clonedImage.Save(saveFileDialog1.FileName, saveFormat);
            }
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {

        }


        //HELPER METHODS
        private void SortTheDarkBits()
        {
            ///STEP 0
            //If we have loadedImage
            if (loadedImage != null)
            {
                //prepare newImage of the same size (we are already doing this on image load)
                //clonedImage = new Bitmap(loadedImage.Size.Width, loadedImage.Size.Height);

                ///STEP 1
                //For each row x
                for (int x = 0; x < loadedImage.Size.Width - 1; x++)
                {
                    //Console.WriteLine("Parsing column " + x + " of " + loadedImage.Size.Width);
                    //Initialize newImage row x to -1s
                    InitializeNewColumn(x, Color.White);
                    //Initialize sorting column    
                    InitializeSortingArray();

                    ///Step 2
                    //For each pixel in column
                    for (int y = 0; y < loadedImage.Size.Height - 1; y++)
                    {

                        //Prepare the current pixel to be used
                        Color currPixel = loadedImage.GetPixel(x, y);

                        //if pixel does not meet sorting condition
                        if (currPixel.GetBrightness() > darkThreshold)
                        {
                            //copy pixel to newImage at same position
                            clonedImage.SetPixel(x, y, currPixel);
                            //if sorting column is not fresh, then we now have a group to sort
                            if (!toSortDarkFresh)
                            {
                                //so sort the sorting column
                                //Console.WriteLine("Sorting...");
                                Quicksort(toSortDark, 0, currDarkIndex - 1);

                                //and copy the now sorted array to newImage at the SAVED SORTING INDEX
                                ApplySorted(x, currDarkPos);
                                //Initialize all of sortingArray to maximum of sorting condition        
                                InitializeSortingArray();
                            }
                        }
                        //else if pixel DOES meet sorting condition
                        else if (currPixel.GetBrightness() <= darkThreshold)
                        {
                            //if sorting column is initialized we have our FIRST point in the current chunk of items to sort
                            if (toSortDarkFresh)
                            {
                                //We start a new sortingArray
                                //by saving our current position as the currentSortPos
                                currDarkPos = y;
                                //and set sort array to no longer fresh
                                toSortDarkFresh = false;
                            }
                            //finally copy pixel to sorting column and iterate the sort index accordingly
                            toSortDark[currDarkIndex++] = currPixel;

                        }
                    }

                    if (!toSortDarkFresh)
                    {
                        //If we get to the top, and there are unsorted elements, sort them before moving on
                        Quicksort(toSortDark, 0, currDarkIndex - 1);
                        //White space problem fix
                        ApplySorted(x, currDarkPos);
                    }
                    pictureBox1.Image = clonedImage;
                    pictureBox1.Update();
                }
                Console.WriteLine("Finished sorting image!");
                Console.WriteLine("Displaying Image...");
                //Display image here
                pictureBox1.Image = clonedImage;
                pictureBox1.Update();
            }
        }

        private void SortTheRedBits()
        {
            ///STEP 0
            //If we have loadedImage
            if (loadedImage != null)
            {
                //prepare newImage of the same size (we are already doing this on image load)
                //clonedImage = new Bitmap(loadedImage.Size.Width, loadedImage.Size.Height);

                ///STEP 1
                //For each row x
                for (int x = 0; x < loadedImage.Size.Width - 1; x++)
                {
                    //Console.WriteLine("Parsing column " + x + " of " + loadedImage.Size.Width);
                    //Initialize newImage row x to -1s
                    InitializeNewColumn(x, Color.White);
                    //Initialize sorting column    
                    InitializeSortingArray();

                    ///Step 2
                    //For each pixel in column
                    for (int y = 0; y < loadedImage.Size.Height - 1; y++)
                    {

                        //Prepare the current pixel to be used
                        Color currPixel = loadedImage.GetPixel(x, y);

                        //if pixel does not meet sorting condition
                        if (currPixel.R < redThreshold)
                        {
                            //copy pixel to newImage at same position
                            clonedImage.SetPixel(x, y, currPixel);
                            //if sorting column is not fresh, then we now have a group to sort
                            if (!toSortDarkFresh)
                            {
                                //so sort the sorting column
                                //Console.WriteLine("Sorting...");
                                QuicksortRed(toSortDark, 0, currDarkIndex - 1);

                                //and copy the now sorted array to newImage at the SAVED SORTING INDEX
                                ApplySorted(x, currDarkPos);
                                //Initialize all of sortingArray to maximum of sorting condition        
                                InitializeSortingArray();
                            }
                        }
                        //else if pixel DOES meet sorting condition
                        else if (currPixel.R >= redThreshold)
                        {
                            //if sorting column is initialized we have our FIRST point in the current chunk of items to sort
                            if (toSortDarkFresh)
                            {
                                //We start a new sortingArray
                                //by saving our current position as the currentSortPos
                                currDarkPos = y;
                                //and set sort array to no longer fresh
                                toSortDarkFresh = false;
                            }
                            //finally copy pixel to sorting column and iterate the sort index accordingly
                            toSortDark[currDarkIndex++] = currPixel;

                        }
                    }

                    if (!toSortDarkFresh)
                    {
                        //If we get to the top, and there are unsorted elements, sort them before moving on
                        QuicksortRed(toSortDark, 0, currDarkIndex - 1);
                        //White space problem fix
                        ApplySorted(x, currDarkPos);
                    }
                    pictureBox1.Image = clonedImage;
                    pictureBox1.Update();
                }
                Console.WriteLine("Finished sorting image by red bits!");
                Console.WriteLine("Displaying Image...");
                //Display image here
                pictureBox1.Image = clonedImage;
                pictureBox1.Update();
            }
        }

        private void SortTheColours()
        {
            ///STEP 0
            //If we have loadedImage
            if (loadedImage != null)
            {
                //prepare newImage of the same size (we are already doing this on image load)
                //clonedImage = new Bitmap(loadedImage.Size.Width, loadedImage.Size.Height);

                ///STEP 1
                //For each row x
                for (int x = 0; x < loadedImage.Size.Width - 1; x++)
                {
                    //Console.WriteLine("Parsing column " + x + " of " + loadedImage.Size.Width);
                    //Initialize newImage row x to -1s
                    float[] outRColumn = new float[loadedImage.Size.Height];
                    float[] outGColumn = new float[loadedImage.Size.Height];
                    float[] outBColumn = new float[loadedImage.Size.Height];

                    Color[] outColumn = new Color[loadedImage.Size.Height];

                    //InitializeNewColumn(x, Color.White);
                    //Initialize sorting columns    
                    InitializeColourSortingArrayR();
                    InitializeColourSortingArrayG();
                    InitializeColourSortingArrayB();

                    ///Step 2
                    //For each pixel in column
                    for (int y = 0; y < loadedImage.Size.Height - 1; y++)
                    {
                        //Split the Column into 3 for R G and B
                        //Prepare the current colors to be checked
                        Color currPixel = loadedImage.GetPixel(x, y);
                        float currR     = currPixel.R;
                        float currG     = currPixel.G;
                        float currB     = currPixel.B;
                        
                        //RED
                        //if Red pixel does not meet sorting condition
                        if (currR < redThreshold)
                        {
                            //copy pixel to outColumn at same position
                            outRColumn[y] = currR;
                            
                            //clonedImage.SetPixel(x, y, currPixel);
                            
                            //if sorting column is not fresh, then we now have a group to sort
                            if (!toSortRedFresh)
                            {
                                //so sort the sorting column
                                //Console.WriteLine("Sorting...");
                                QuicksortFloat(toSortRed, 0, currRedIndex - 1);

                                //and copy the now sorted array to newImage at the SAVED SORTING INDEX
                                ApplySortedFloat(currRedPos, currRedIndex, toSortRed, outRColumn);
                                //Initialize all of sortingArray to maximum of sorting condition        
                                InitializeColourSortingArrayR();
                            }
                        }
                        //else if pixel DOES meet sorting condition
                        else if (currR >= redThreshold)
                        {
                            //if sorting column is initialized we have our FIRST point in the current chunk of items to sort
                            if (toSortRedFresh)
                            {
                                //We start a new sortingArray
                                //by saving our current position as the currentSortPos
                                currRedPos = y;
                                //and set sort array to no longer fresh
                                toSortRedFresh = false;
                            }
                            //finally copy pixel to sorting column and iterate the sort index accordingly
                            toSortRed[currRedIndex++] = currR;

                        }


                        //GREEN
                        if (currG < greenThreshold)
                        {
                            //copy pixel to outColumn at same position
                            outGColumn[y] = currG;

                            //clonedImage.SetPixel(x, y, currPixel);

                            //if sorting column is not fresh, then we now have a group to sort
                            if (!toSortGreenFresh)
                            {
                                //so sort the sorting column
                                //Console.WriteLine("Sorting...");
                                QuicksortFloat(toSortGreen, 0, currGreenIndex - 1);

                                //and copy the now sorted array to newImage at the SAVED SORTING INDEX
                                ApplySortedFloat(currGreenPos, currGreenIndex, toSortGreen, outGColumn);
                                //Initialize all of sortingArray to maximum of sorting condition
                                InitializeColourSortingArrayG();
                            }
                        }
                        //else if pixel DOES meet sorting condition
                        else if (currG >= greenThreshold)
                        {
                            //if sorting column is initialized we have our FIRST point in the current chunk of items to sort
                            if (toSortGreenFresh)
                            {
                                //We start a new sortingArray
                                //by saving our current position as the currentSortPos
                                currGreenPos = y;
                                //and set sort array to no longer fresh
                                toSortGreenFresh = false;
                            }
                            //finally copy pixel to sorting column and iterate the sort index accordingly
                            toSortGreen[currGreenIndex++] = currG;

                        }

                        //BLUE
                        if (currB < blueThreshold)
                        {
                            //copy pixel to outColumn at same position
                            outBColumn[y] = currB;

                            //clonedImage.SetPixel(x, y, currPixel);

                            //if sorting column is not fresh, then we now have a group to sort
                            if (!toSortBlueFresh)
                            {
                                //so sort the sorting column
                                //Console.WriteLine("Sorting...");
                                QuicksortFloat(toSortBlue, 0, currBlueIndex - 1);

                                //and copy the now sorted array to newImage at the SAVED SORTING INDEX
                                ApplySortedFloat(currBluePos, currBlueIndex, toSortBlue, outBColumn);
                                //Initialize all of sortingArray to maximum of sorting condition
                                InitializeColourSortingArrayB();
                            }
                        }
                        //else if pixel DOES meet sorting condition
                        else if (currB >= blueThreshold)
                        {
                            //if sorting column is initialized we have our FIRST point in the current chunk of items to sort
                            if (toSortBlueFresh)
                            {
                                //We start a new sortingArray
                                //by saving our current position as the currentSortPos
                                currBluePos = y;
                                //and set sort array to no longer fresh
                                toSortBlueFresh = false;
                            }
                            //finally copy pixel to sorting column and iterate the sort index accordingly
                            toSortBlue[currBlueIndex++] = currB;

                        }

                    }

                    if (!toSortRedFresh)
                    {
                        //If we get to the top, and there are unsorted elements, sort them before moving on
                        QuicksortFloat(toSortRed, 0, currRedIndex - 1);
                        //White space problem fix
                        ApplySortedFloat(currRedPos, currRedIndex, toSortRed, outRColumn);
                    }
                    if (!toSortGreenFresh)
                    {
                        //If we get to the top, and there are unsorted elements, sort them before moving on
                        QuicksortFloat(toSortGreen, 0, currGreenIndex - 1);
                        //White space problem fix
                        ApplySortedFloat(currGreenPos, currGreenIndex, toSortGreen, outGColumn);
                    }
                    if (!toSortBlueFresh)
                    {
                        //If we get to the top, and there are unsorted elements, sort them before moving on
                        QuicksortFloat(toSortBlue, 0, currBlueIndex - 1);
                        //White space problem fix
                        ApplySortedFloat(currBluePos, currBlueIndex, toSortBlue, outBColumn);
                    }
                    //Collaborate Arrays into one outArray
                    outColumn = CollaborateSortedRGB(outRColumn, outGColumn, outBColumn);
                    //Set image's current column to outArray
                    for (int i = 0; i < loadedImage.Size.Height; i++)
                    {
                        clonedImage.SetPixel(x, i, outColumn[i]);
                    }
                    //Modify this to draw properly
                    pictureBox1.Image = clonedImage;
                    pictureBox1.Update();
                }
                Console.WriteLine("Finished sorting image by red bits!");
                Console.WriteLine("Displaying Image...");
                //Display image here
                pictureBox1.Image = clonedImage;
                pictureBox1.Update();
            }
        }

        private void InitializeNewColumn(int columnIndex, Color initColour)
        {
            //Console.WriteLine("Initializing new column of image");
            //For each pixel in the new image's current column
            for (int i = 0; i < loadedImage.Size.Height - 1; i++)
            {
                //set the pixel to our initial pixel, white, our maximum brightness
                clonedImage.SetPixel(columnIndex, i, initColour);
            }
        }

        private void InitializeSortingArray()
        {
            //Console.WriteLine("Initializing Sorting Array...");
            //Initialize the sorting array's size to be the height of the image
            toSortDark = new Color[loadedImage.Size.Height];
            currDarkIndex = 0;
            //and set each item in the sorting array
            for (int i = 0; i < loadedImage.Size.Height - 1; i++)
            {
                //To our minimum brightness pixel, which we is black;
                toSortDark[i] = (maxPixel);
            }
            toSortDarkFresh = true;
        }

        private void InitializeColourSortingArrayR()
        {
            //Console.WriteLine("Initializing Sorting Array...");
            //Initialize the sorting array's size to be the height of the image
            
            toSortRed       = new float[loadedImage.Size.Height];

            currRedIndex    = 0;

            //and set each item in the sorting array
            for (int i = 0; i < loadedImage.Size.Height - 1; i++)
            {
                //To our minimum brightness pixel, which we is black;
                toSortRed[i]    = (255);
            }
            toSortRedFresh      = true;
        }

        private void InitializeColourSortingArrayG()
        {
            //Console.WriteLine("Initializing Sorting Array...");
            //Initialize the sorting array's size to be the height of the image

            
            toSortGreen = new float[loadedImage.Size.Height];
            

            
            currGreenIndex = 0;
            

            //and set each item in the sorting array
            for (int i = 0; i < loadedImage.Size.Height - 1; i++)
            {
                //To our minimum brightness pixel, which we is black;
            
                toSortGreen[i] = (255);
            
            }
            
            toSortGreenFresh = true;
            
        }

        private void InitializeColourSortingArrayB()
        {
            //Console.WriteLine("Initializing Sorting Array...");
            //Initialize the sorting array's size to be the height of the image

            
            toSortBlue = new float[loadedImage.Size.Height];

            currBlueIndex = 0;

            //and set each item in the sorting array
            for (int i = 0; i < loadedImage.Size.Height - 1; i++)
            {
                //To our minimum brightness pixel, which we is black;
                toSortBlue[i] = (255);
            }
            toSortBlueFresh = true;
        }


        private void QuicksortRed(Color[] arrayToSort, int left, int right)
        {
            //Console.WriteLine("Sorting array from " + left + " to " + right);
            //Lower and upper selectors
            int iLower = left;
            int iUpper = right;

            //Select a random pivot, but not the first element
            float fPivot = arrayToSort[(left + right) / 2].R;

            //Partition array into sections above and below pivot
            while (iLower <= iUpper)
            {

                while (arrayToSort[iLower].R > fPivot)
                {
                    iLower++;
                }


                while (arrayToSort[iUpper].R < fPivot)
                {
                    iUpper--;
                }


                //Swap the entries at the lower and upper indices
                if (iLower <= iUpper)
                {
                    //Swap
                    Color tmpPixel = arrayToSort[iLower];
                    arrayToSort[iLower] = arrayToSort[iUpper];
                    arrayToSort[iUpper] = tmpPixel;

                    iLower++;
                    iUpper--;
                }
            }
            //Recursively call partition on each partition
            if (left < iUpper)
            {
                QuicksortRed(arrayToSort, left, iUpper);
            }

            if (iLower < right)
            {
                QuicksortRed(arrayToSort, iLower, right);
            }
        }

        private void QuicksortFloat(float[] arrayToSort, int left, int right)
        {
            //Console.WriteLine("Sorting array from " + left + " to " + right);
            //Lower and upper selectors
            int iLower = left;
            int iUpper = right;

            //Select a random pivot, but not the first element
            float fPivot = arrayToSort[(left + right) / 2];

            //Partition array into sections above and below pivot
            while (iLower <= iUpper)
            {

                while (arrayToSort[iLower] > fPivot)
                {
                    iLower++;
                }


                while (arrayToSort[iUpper] < fPivot)
                {
                    iUpper--;
                }


                //Swap the entries at the lower and upper indices
                if (iLower <= iUpper)
                {
                    //Swap
                    float tmpPixel = arrayToSort[iLower];
                    arrayToSort[iLower] = arrayToSort[iUpper];
                    arrayToSort[iUpper] = tmpPixel;

                    iLower++;
                    iUpper--;
                }
            }
            //Recursively call partition on each partition
            if (left < iUpper)
            {
                QuicksortFloat(arrayToSort, left, iUpper);
            }

            if (iLower < right)
            {
                QuicksortFloat(arrayToSort, iLower, right);
            }
        }

        private void Quicksort(Color[] arrayToSort, int left, int right)
        {
            //Console.WriteLine("Sorting array from " + left + " to " + right);
            //Lower and upper selectors
            int iLower = left;
            int iUpper = right;

            //Select a random pivot, but not the first element
            float fPivot = arrayToSort[(left + right) / 2].GetBrightness();
            
            //Partition array into sections above and below pivot
            while (iLower <= iUpper)
            {

                 while (arrayToSort[iLower].GetBrightness() < fPivot)
                { 
                    iLower++;
                }


                while (arrayToSort[iUpper].GetBrightness() > fPivot)
                {
                    iUpper--;
                }
                

                //Swap the entries at the lower and upper indices
                if (iLower <= iUpper)
                {
                    //Swap
                    Color tmpPixel = arrayToSort[iLower];
                    arrayToSort[iLower] = arrayToSort[iUpper];
                    arrayToSort[iUpper] = tmpPixel;

                    iLower++;
                    iUpper--;
                }
            }
            //Recursively call partition on each partition
            if (left < iUpper)
            {
                Quicksort(arrayToSort, left, iUpper);
            }

            if (iLower < right)
            {
                Quicksort(arrayToSort, iLower, right);
            }
        }

        private void ApplySorted(int column, int sortPos)
        {
            //Implement here reinserting a sorted array at the given position
            //Console.WriteLine("Applying sorted section to new image");
            //for each position in our new image, starting at index "column, sortPos"
            for(int j = 0; j < currDarkIndex; j++)
            {
                //copy the nth element in the sorted array into the (n + sortPos)th position of column
                clonedImage.SetPixel(column, j + sortPos, toSortDark[j]);
            }
            pictureBox1.Image = clonedImage;
            //InitializeSortingArray();
        }

        private void ApplySortedFloat(int sortPos, int currIndex, float[] toSortArray, float[] outArray)
        {
            //Implement here reinserting a sorted array at the given position
            //Console.WriteLine("Applying sorted section to new image");
            //for each position in our new image, starting at index "column, sortPos"
            for (int j = 0; j < currIndex; j++)
            {
                //copy the nth element in the sorted array into the (n + sortPos)th position of column
                outArray[j + sortPos] = toSortArray[j];
            }
            
            //pictureBox1.Image = clonedImage;
            //InitializeSortingArray();
        }

        private Color[] CollaborateSortedRGB(float[] inR, float[] inG, float[] inB)
        {
            Color[] outRGB = new Color[loadedImage.Size.Height];

            for (int i = 0; i < loadedImage.Size.Height; i++)
            {
                outRGB[i] = Color.FromArgb((int)inR[i], (int)inG[i], (int)inB[i]);
            }
            return outRGB;
        }
    }
}
