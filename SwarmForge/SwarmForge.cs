using System;
using System.Linq;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms.DataVisualization.Charting;

namespace SwarmForge
{
    public partial class SwarmForge : Form
    {
        // define costMatrix as global variable
        int[][] costMatrix;
        int opt;
        //initialize the component
        public SwarmForge() { InitializeComponent(); }


        // actions on application load
        public void SwarmForge_Load(object sender, EventArgs e)
        {
            // for each of the data files located in folder "data" populate combobox with their names
            foreach (string file in Directory.EnumerateFiles("data/", "*.txt"))
            {
                string filetrim = file.Substring(5);
                filetrim = filetrim.Remove(filetrim.Length - 4);
                comboBox1.Items.Add(filetrim);
            }
            // default selected combobox item is set to first one
            comboBox1.SelectedIndex = 0;

            // initialize slider values to center and populate labels
            C0.Value = 50;
            valC0.Text = Convert.ToString(C0.Value) + "%";
            Cglobal.Value = 50;
            valCglobal.Text = Convert.ToString(Cglobal.Value) + "%";
            Clocal.Value = 50;
            valClocal.Text = Convert.ToString(Clocal.Value) + "%";
            Ciner.Value = 50;
            valCiner.Text = Convert.ToString(Ciner.Value) + "%";

            // check p-median score as default goal
            medi_rad.Checked = true;
            // check iteration limit as default stop
            iteration_rad.Checked = true;

            // initialize numeric field extremes
            particleN.Minimum = 2;
            particleN.Maximum = 10000000; //10kk particles
            maxV.Minimum = 1;
            maxV0.Minimum = 1;
            // unused localRad.Minimum = 1;
            // unused localRad.Maximum = 100000; //100k 
            result_limit.Minimum = 0;
            result_limit.Maximum = 1000000; //1kk percent of optimum value
            time_limit.Minimum = 1;
            time_limit.Maximum = 1000000; //1kk seconds
            iter_limit.Minimum = 1;
            iter_limit.Maximum = 1000000; //1kk seconds
        }



    // DESIGN ONLY - Complementary trackbars change log - if one is changed keep them complementary {
        private void Cglobal_ValueChanged(object sender, EventArgs e)
        {
            valCglobal.Text = Convert.ToString(Cglobal.Value) + "%";
            Clocal.Value = 100 - Cglobal.Value;
            valClocal.Text = Convert.ToString(Clocal.Value) + "%";
        }

        private void Clocal_ValueChanged(object sender, EventArgs e)
        {
            valClocal.Text = Convert.ToString(Clocal.Value) + "%";
            Cglobal.Value = 100 - Clocal.Value;
            valCglobal.Text = Convert.ToString(Cglobal.Value) + "%";
        }

        private void C0_ValueChanged(object sender, EventArgs e)
        {
            valC0.Text = Convert.ToString(C0.Value) + "%";
            Ciner.Value = 100 - C0.Value;
            valCiner.Text = Convert.ToString(Ciner.Value) + "%";
        }

        private void Ciner_ValueChanged(object sender, EventArgs e)
        {
            valCiner.Text = Convert.ToString(Ciner.Value) + "%";
            C0.Value = 100 - Ciner.Value;
            valC0.Text = Convert.ToString(C0.Value) + "%";
        }



    // FETCH BUTTON ACTION - DATASET PREPROCESSING
        public void fetch_button_Click(object sender, EventArgs e)
        {
            // hide panels (might be hidden already)
            solve_pan.Visible = false;
            current_pan.Visible = false;

            // get database selection (must be selected)
            string choice = comboBox1.SelectedItem.ToString();

            // get optimal p center from results file for it and store it in label
            string[] centTxt = File.ReadAllText("opt/cent.txt").Split(new[] { choice + '=' }, StringSplitOptions.None);
            int cent = Convert.ToInt32(centTxt[1].Split('\n')[0]);
            cent_lab.Text = Convert.ToString(cent);

            // get optimal p median from results file for it and store it in label
            string[] mediTxt = File.ReadAllText("opt/med.txt").Split(new[] { choice + '=' }, StringSplitOptions.None);
            int medi = Convert.ToInt32(mediTxt[1].Split('\n')[0]);
            medi_lab.Text = Convert.ToString(medi);

            // get location cost matrix for selected dataset
            // create chosen dataset filepath
            choice = "data/" + choice + ".txt";
            // convert it using  ConvertMatrix function from this solution
            costMatrix = ConvertMatrix(choice);

            // laod panels (make them visible)          
            basic_pan.Visible = true;
            particle_pan.Visible = true;
            influ_pan.Visible = true;
            reconst_pan.Visible = true;
            limit_pan.Visible = true;
            solve_pan.Visible = true;
            current_pan.Visible = true;

            // initialize numeric field extremes
            maxV.Maximum = Convert.ToInt32(locationN.Text) / 2;
            maxV0.Maximum = Convert.ToInt32(locationN.Text) / 2;
            maxR.Maximum = Convert.ToInt32(locationN.Text) / 2;
            constTo.Maximum = Convert.ToInt32(locationN.Text);

            // chart initialize

            //chart.Series.Clear();           

            // initialize three series
            //var ser_best = new System.Windows.Forms.DataVisualization.Charting.Series
            //{
            //    Name = "Best Fitness so far",
            //    Color = System.Drawing.Color.Red,
            //    IsVisibleInLegend = false,
            //    IsXValueIndexed = true,
            //    ChartType = SeriesChartType.Line
            //};
            //this.chart.Series.Add(ser_best);

            //var ser_now = new System.Windows.Forms.DataVisualization.Charting.Series
            //{
            //    Name = "Best Current Fitness",
            //    Color = System.Drawing.Color.Green,
            //    IsVisibleInLegend = false,
            //    IsXValueIndexed = true,
            //    ChartType = SeriesChartType.Line
            //};
            //this.chart.Series.Add(ser_now);

            //var avg_now = new System.Windows.Forms.DataVisualization.Charting.Series
            //{
            //    Name = "Current Fitness Average",
            //    Color = System.Drawing.Color.Yellow,
            //    IsVisibleInLegend = false,
            //    IsXValueIndexed = true,
            //    ChartType = SeriesChartType.Line
            //};
            //this.chart.Series.Add(avg_now);

        }

        // RECONSTRUCT BUTTON ACTION - DATASET REPROCESSING
        private void recon_Click(object sender, EventArgs e)
        {
            // get location number
            int locationN = Convert.ToInt32(this.locationN.Text);

            // transform generated matrix so that the close locations are marked with aproximate numbers

            // initialize a help matrix
            int[][] hMatrix = new int[locationN + 1][];
            for (int i = 1; i < locationN + 1; i++)
            {
                hMatrix[i] = new int[locationN + 1];
            }

            //copy original matrix into help one
            for (int i = 1; i < locationN + 1; i++)
            {
                for (int j = 1; j < locationN + 1; j++)
                {
                    hMatrix[i][j] = costMatrix[i][j];
                }
            }

            // initialize mapping array
            int[] mapping = new int[locationN + 1];

            // for each location set cost to zeroth location to avoid min function interfirance
            for (int i = 1; i < locationN + 1; i++)
            {
                hMatrix[i][0] = 1000;
            }

            // get wanted location to start reconstruction
            int constructTo = Convert.ToInt32(constTo.Value);

            // set mapping of 1 to 1 (number one is a starting point - in this case, but it can be any number i guess)
            mapping[1] = constructTo;

            // set current closest to 1
            int close = constructTo;

            // for each location remove number 1 as an option (set as a high cost) since it is already used
            for (int i = 1; i < locationN + 1; i++)
            {
                //(also remove option of lowest cost to be self)
                hMatrix[i][i] = 1000;
                hMatrix[i][constructTo] = 1000;
            }
            // perform one progress bar step
            progressBar1.PerformStep();

            // for each next closest number
            for (int i = 2; i < locationN + 1; i++)
            {
                // find its closest
                close = Array.IndexOf(hMatrix[close], hMatrix[close].Min());

                //store into mapping array
                mapping[i] = close;

                // remove as option to all others
                for (int j = 1; j < locationN + 1; j++)
                {
                    hMatrix[j][close] = 1000;
                }
                // perform progress step for each line done
                progressBar1.PerformStep();
            }

            //copy original matrix into help one again
            for (int i = 1; i < locationN + 1; i++)
            {
                for (int j = 1; j < locationN + 1; j++)
                {
                    hMatrix[i][j] = costMatrix[i][j];
                }
            }

            // reconstruct matrix using mapping
            for (int i = 1; i < locationN + 1; i++)
            {
                for (int j = 1; j < locationN + 1; j++)
                {
                    costMatrix[i][j] = hMatrix[mapping[i]][mapping[j]];
                }

                // perform progress step for each line done
                progressBar1.PerformStep();
            }

            // get construction index of curent matrix
            int matconstr = 0;
            // sum consts of all adjacent locations
            for (int i = 1; i < locationN; i++)
            {
                matconstr = matconstr + costMatrix[i][i + 1];
            }
            label13.Text = Convert.ToString(matconstr);
        }

        // STOP BUTTON ACTION - STOP THE SEARCH
        private void stop_Click(object sender, EventArgs e)
        {

        }

        // GO BUTTON ACTION - INTELLIGENT SOLUTION SEARCH
        private void go_but_Click(object sender, EventArgs e)
        {

            // clear chart
            chart.Series["best_fit"].Points.Clear();
            chart.Series["best_now"].Points.Clear();
            chart.Series["avg_now"].Points.Clear();
            chart.Series["optimum"].Points.Clear();

            //get values from labels
            //particle number
            int pN = Convert.ToInt32(particleN.Value);
            //object number
            int oN = Convert.ToInt32(objectN.Text);
            //location number
            int lN = Convert.ToInt32(locationN.Text);
            //maximum initial particle speed
            int mV0 = Convert.ToInt32(maxV0.Value);
            //maximum particle speed
            int mV = Convert.ToInt32(maxV.Value);

            //get values from sliders
            // initial inertia influence
            double Co = C0.Value / 100.00;
            // other particles influence
            double Ci = Ciner.Value / 100.00;
            // local best particle position
            double Cl = (Clocal.Value / 100.00) * Ci;
            // global best particle position
            double Cg = (Cglobal.Value / 100.00) * Ci;

            //chart initialize
            if (cent_rad.Checked == true)
            {
                opt = Convert.ToInt32(cent_lab.Text);
                this.chart.ChartAreas["plot"].AxisY.Minimum = Math.Floor(opt / 50.00) * 50;
            }
            else
            {
                opt = Convert.ToInt32(medi_lab.Text);
                this.chart.ChartAreas["plot"].AxisY.Minimum = Math.Floor(opt / 500.00) * 500;
            } 

            
            this.chart.ChartAreas["plot"].AxisY2.Minimum = this.chart.ChartAreas["plot"].AxisY.Minimum;
            this.chart.ChartAreas["plot"].AxisX.Minimum = 1;
            this.chart.ChartAreas["plot"].AxisX.Maximum = Convert.ToInt32(iter_limit.Value);

            opt_out.Text = Convert.ToString(opt);

            // initialize random particle matrix
            int[][] pMatrix = new int[pN + 1][];
            for (int i = 1; i < pN + 1; i++)
            {
                pMatrix[i] = new int[oN + 1];
            }
            pMatrix = RandomParticleMatrix(pN, oN, lN);

            // initialize random particle inertion matrix
            int[][] vMatrix = new int[pN + 1][];
            for (int i = 1; i < pN + 1; i++)
            {
                vMatrix[i] = new int[oN + 1];
            }
            vMatrix = RandomInertioneMatrix(pN, oN, mV0);

            //initialize curren particle inertiion matrix(now same as initial)
            int[][] nvMatrix = new int[pN + 1][];
            for (int i = 1; i < pN + 1; i++)
            {
                nvMatrix[i] = new int[oN + 1];
            }
            nvMatrix = vMatrix;

            //int[][] nvMatrix = vMatrix;

            // initialize fitness array
            int[] fit = new int[pN];
            double fitavg = 0;

            //initialize local best matrix (current positions are now also best ones)
            int[][] lMatrix = new int[pN + 1][];
            for (int i = 1; i < pN + 1; i++)
            {
                lMatrix[i] = new int[oN + 1];
            }
            lMatrix = pMatrix;

            // initialize local fitness vector
            int[] lfit = new int[pN];

            // initialize stopwatch
            var timer = Stopwatch.StartNew();

            // initialize log
            string log = "";

            // set iteration number to 0
            int c = 0;

            // SIX DIFFERENT ALGORITHMS FOLLOW - DEFINED BY STOP FUNCTION AND RESULT WANTED

            // 1. P-median result with loop stoping on iteration limit hit
            // 2. P-median result with loop stoping on needed result hit
            // 3. P-median result with loop stoping on time limit hit

            // 4. P-center result with loop stoping on iteration limit hit
            // 5. P-center result with loop stoping on needed result hit
            // 6. P-center result with loop stoping on time limit hit

            // Although theese are relatively similar and could be put together,
            // they were divided in orther to avoid if statement check inside the while loops,
            // as there repeat number tends to be very high.

            // IF MEDIAN IS NEEDED - 1, 2, and 3
            if (medi_rad.Checked == true)
            {
                // calcualate first fintess
                fit = MatrixFitnessMed(pMatrix, pN, lN, oN, costMatrix);
                lfit = fit;

                // global best fitness and soulution assign
                int best = fit.Min();
                int nbest = 99999;
                
                nbest = best;
                int[] vbest = pMatrix[Array.IndexOf(fit, best)];

                // 1. P-median result with loop stoping on iteration limit hit
                if (iteration_rad.Checked == true)
                {
                    int x = Convert.ToInt32(iter_limit.Value);
                    while (c < x)
                    {
                        // increase iteration number
                        c++;
                        // perform a step
                        var newstep = step(pMatrix, lMatrix, vbest, vMatrix, nvMatrix, pN, oN, Co, Cl, Cg, mV, lN);
                        pMatrix = newstep.Item1;
                        nvMatrix = newstep.Item2;

                        // calculate fintess
                        fit = MatrixFitnessMed(pMatrix, pN, lN, oN, costMatrix);

                        // calculate average fitness
                        fitavg = 0;
                        for (int i = 1; i < pN+1; i++)
                        {
                            fitavg = fitavg + fit[i];
                        }
                        fitavg = fitavg / pN;

                        // best fitness
                        nbest = fit.Min();
                        if (nbest < best)
                        {
                            best = nbest;
                            vbest = pMatrix[Array.IndexOf(fit, nbest)];
                        }
                        // best solution vector

                        // set local bests matrix and value vector
                        for (int i = 1; i < pN + 1; i++)
                        {
                            // for each particle if new fitness is better then last best
                            // replace in local best aprticle matrix and local fitness
                            if (fit[i] < lfit[i])
                            {
                                lfit[i] = fit[i];
                                lMatrix[i] = pMatrix[i];
                            }
                        }

                        // log
                        log = Log(log, pMatrix, nvMatrix, vMatrix, fit, pN, oN);

                        // plot
                        Solution(c, timer.ElapsedMilliseconds, best, nbest, fitavg);
                    }
                }

                // 2. P-median result with loop stoping on needed result hit
                if (result_rad.Checked == true)
                {
                    int x = Convert.ToInt32(medi_lab.Text);
                    x = x * (1 + (Convert.ToInt32(result_limit.Value) / 100));
                    while (best > x)
                    {
                        // increase iteration number
                        c++;

                        // perform a step
                        var newstep = step(pMatrix, lMatrix, vbest, vMatrix, nvMatrix, pN, oN, Co, Cl, Cg, mV, lN);
                        pMatrix = newstep.Item1;
                        nvMatrix = newstep.Item2;

                        // calculate fintess
                        fit = MatrixFitnessMed(pMatrix, pN, lN, oN, costMatrix);

                        // calculate average fitness
                        fitavg = 0;
                        for (int i = 1; i < pN + 1; i++)
                        {
                            fitavg = fitavg + fit[i];
                        }
                        fitavg = fitavg / pN;

                        // best fitness
                        nbest = fit.Min();
                        if (nbest < best)
                        {
                            best = nbest;
                            vbest = pMatrix[Array.IndexOf(fit, nbest)];
                        }

                        // set local bests matrix and value vector
                        for (int i = 1; i < pN + 1; i++)
                        {
                            // for each particle if new fitness is better then last best
                            // replace in local best aprticle matrix and local fitness
                            if (fit[i] < lfit[i])
                            {
                                lfit[i] = fit[i];
                                lMatrix[i] = pMatrix[i];
                            }
                        }

                        // log
                        log = Log(log, pMatrix, nvMatrix, vMatrix, fit, pN, oN);

                        // plot
                        Solution(c, timer.ElapsedMilliseconds, best, nbest, fitavg);
                    }
                }

                // 3. P-median result with loop stoping on time limit hit
                if (time_rad.Checked == true)
                {
                    int x = Convert.ToInt32(time_limit.Value) * 1000;
                    while (timer.ElapsedMilliseconds < x)
                    {
                        // increase iteration number
                        c++;

                        // perform a step
                        var newstep = step(pMatrix, lMatrix, vbest, vMatrix, nvMatrix, pN, oN, Co, Cl, Cg, mV, lN);
                        pMatrix = newstep.Item1;
                        nvMatrix = newstep.Item2;

                        // calculate fintess
                        fit = MatrixFitnessMed(pMatrix, pN, lN, oN, costMatrix);

                        // calculate average fitness
                        fitavg = 0;
                        for (int i = 1; i < pN + 1; i++)
                        {
                            fitavg = fitavg + fit[i];
                        }
                        fitavg = fitavg / pN;

                        // best fitness
                        nbest = fit.Min();
                        if (nbest < best)
                        {
                            best = nbest;
                            vbest = pMatrix[Array.IndexOf(fit, nbest)];
                        }

                        // set local bests matrix and value vector
                        for (int i = 1; i < pN + 1; i++)
                        {
                            // for each particle if new fitness is better then last best
                            // replace in local best aprticle matrix and local fitness
                            if (fit[i] < lfit[i])
                            {
                                lfit[i] = fit[i];
                                lMatrix[i] = pMatrix[i];
                            }
                        }

                        // log
                        log = Log(log, pMatrix, nvMatrix, vMatrix, fit, pN, oN);

                        // plot
                        Solution(c, timer.ElapsedMilliseconds, best, nbest, fitavg);
                    }
                }
            }
            // IF CENTER IS NEEDED - 4, 5 and 6
            else
            {
                // calcualate first fintess
                fit = MatrixFitnessCent(pMatrix, pN, lN, oN, costMatrix);
                fit[0] = 999999;

                // global best fitness and soulution assign
                int best = fit.Min();
                int nbest = 99999;
                nbest = best;
                lfit = fit;

                // best solution vector
                int[] vbest = pMatrix[Array.IndexOf(fit, best)];

                // 4. P-center result with loop stoping on iteration limit hit
                if (iteration_rad.Checked == true)
                {
                    int x = Convert.ToInt32(iter_limit.Value);
                    while (c < x)
                    {
                        // increase iteration number
                        c++;

                        // perform a step
                        var newstep = step(pMatrix, lMatrix, vbest, vMatrix, nvMatrix, pN, oN, Co, Cl, Cg, mV, lN);
                        pMatrix = newstep.Item1;
                        nvMatrix = newstep.Item2;

                        // calculate fintess
                        fit = MatrixFitnessCent(pMatrix, pN, lN, oN, costMatrix);

                        // calculate average fitness
                        fitavg = 0;
                        for (int i = 1; i < pN + 1; i++)
                        {
                            fitavg = fitavg + fit[i];
                        }
                        fitavg = fitavg / pN;

                        // best fitness
                        nbest = fit.Min();
                        if (nbest < best)
                        {
                            best = nbest;
                            vbest = pMatrix[Array.IndexOf(fit, nbest)];
                        }

                        // set local bests matrix and value vector
                        for (int i = 1; i < pN + 1; i++)
                        {
                            // for each particle if new fitness is better then last best
                            // replace in local best aprticle matrix and local fitness
                            if (fit[i] < lfit[i])
                            {
                                lfit[i] = fit[i];
                                lMatrix[i] = pMatrix[i];
                            }
                        }

                        // log
                        log = Log(log, pMatrix, nvMatrix, vMatrix, fit, pN, oN);

                        // plot
                        Solution(c, timer.ElapsedMilliseconds, best, nbest, fitavg);
                    }
                }

                // 5. P-center result with loop stoping on needed result hit
                if (result_rad.Checked == true)
                {
                    int x = Convert.ToInt32(cent_lab.Text) + 1;
                    x = x * (1 + (Convert.ToInt32(result_limit.Value) / 100));
                    while (best > x)
                    {
                        // increase iteration number
                        c++;

                        // perform a step
                        var newstep = step(pMatrix, lMatrix, vbest, vMatrix, nvMatrix, pN, oN, Co, Cl, Cg, mV, lN);
                        pMatrix = newstep.Item1;
                        nvMatrix = newstep.Item2;

                        // calculate fintess
                        fit = MatrixFitnessCent(pMatrix, pN, lN, oN, costMatrix);

                        // calculate average fitness
                        fitavg = 0;
                        for (int i = 1; i < pN + 1; i++)
                        {
                            fitavg = fitavg + fit[i];
                        }
                        fitavg = fitavg / pN;

                        // best fitness
                        nbest = fit.Min();
                        if (nbest < best)
                        {
                            best = nbest;
                            vbest = pMatrix[Array.IndexOf(fit, nbest)];
                        }

                        // set local bests matrix and value vector
                        for (int i = 1; i < pN + 1; i++)
                        {
                            // for each particle if new fitness is better then last best
                            // replace in local best aprticle matrix and local fitness
                            if (fit[i] < lfit[i])
                            {
                                lfit[i] = fit[i];
                                lMatrix[i] = pMatrix[i];
                            }
                        }

                        // log
                        log = Log(log, pMatrix, nvMatrix, vMatrix, fit, pN, oN);

                        // plot
                        Solution(c, timer.ElapsedMilliseconds, best, nbest, fitavg);
                    }
                }

                // 6. P-center result with loop stoping on time limit hit
                if (time_rad.Checked == true)
                {
                    int x = Convert.ToInt32(time_limit.Value) * 1000;
                    while (timer.ElapsedMilliseconds < x)
                    {
                        // increase iteration number
                        c++;

                        // perform a step
                        var newstep = step(pMatrix, lMatrix, vbest, vMatrix, nvMatrix, pN, oN, Co, Cl, Cg, mV, lN);
                        pMatrix = newstep.Item1;
                        nvMatrix = newstep.Item2;

                        // calculate fintess
                        fit = MatrixFitnessCent(pMatrix, pN, lN, oN, costMatrix);

                        // calculate average fitness
                        fitavg = 0;
                        for (int i = 1; i < pN + 1; i++)
                        {
                            fitavg = fitavg + fit[i];
                        }
                        fitavg = fitavg / pN;

                        // best fitness
                        nbest = fit.Min();
                        if (nbest < best)
                        {
                            best = nbest;
                            vbest = pMatrix[Array.IndexOf(fit, nbest)];
                        }
                        // set local bests matrix and value vector
                        for (int i = 1; i < pN + 1; i++)
                        {
                            // for each particle if new fitness is better then last best
                            // replace in local best aprticle matrix and local fitness
                            if (fit[i] < lfit[i])
                            {
                                lfit[i] = fit[i];
                                lMatrix[i] = pMatrix[i];
                            }
                        }

                        // log
                        log = Log(log, pMatrix, nvMatrix, vMatrix, fit, pN, oN);

                        // plot
                        Solution(c, timer.ElapsedMilliseconds, best, nbest, fitavg);
                    }
                }
            }

            // stop the stopwatch
            timer.Stop();

            // this can be used to write a log file - optional
            File.WriteAllText("log/log.txt", log);
        }


    // FUNCTIONS

        //Convert a dataset into cost matrix function (input is dataset filepath)
        public int[][] ConvertMatrix(string file)
        {
            // read the file into string
            string contents = File.ReadAllText(file);

            // split it into lines
            string[] lines = contents.Split('\n');

            // grab the first one, contating parameters
            string[] parameters = lines[0].Split(' ');

            // first parameter is total number of possible locations fill the variable and label
            int locationN = Convert.ToInt32(parameters[1]);
            this.locationN.Text = Convert.ToString(locationN);

            // second one is total number of items that should be scattered
            int objectN = Convert.ToInt32(parameters[3]);
            this.objectN.Text = Convert.ToString(objectN);

            // initialize progress bar (one location dor each matrix line and one line populated is one progress step)
            progressBar1.Value = 0;
            progressBar1.Maximum = locationN*2;
            progressBar1.Step = 1;
            
            // third one is number of lines in the file (not important - commented bellow)
            // unused  int rowsN = Convert.ToInt32(parameters[2]);

            // exclude first line (containig parameters and not costs) from the dataset
            lines = lines.Skip(1).ToArray();

            // create new matrix thet will contain travel expences for each two locations
            int[][] matrix = new int[locationN + 1][];
            for (int i = 1; i < locationN + 1; i++)
            {
                matrix[i] = new int[locationN + 1];
            }

            // for each two locations in the matrix
            for (int i = 1; i < locationN + 1; i++)
            {
                for (int j = 1; j < locationN + 1; j++)
                {
                    // if two are the same transport cost is 0
                    if (i == j) { matrix[i][j] = 0; }
                    // otherwise it's maximum
                    else { matrix[i][j] = 10000; }
                }
            }

            // for each dataset line grab the triplet containing two location and a transport cost between them and replace the maximum cost with it
            foreach (string line in lines)
            {
                // replace the coresponding matrix lines
                if (line != "") {
                    string[] p = line.Split(' ');
                    matrix[Convert.ToInt32(p[1])][Convert.ToInt32(p[2])] = Convert.ToInt32(p[3]);
                }
            }

            // use floyds algoritham to calculate cost of transport betweeen locations not directly connected
            for (int k = 1; k < locationN + 1; k++)
            {
                for (int i = 1; i < locationN + 1; i++)
                {
                    for (int j = 1; j < locationN + 1; j++)
                    {
                        if (matrix[i][j] > matrix[i][k] + matrix[k][j])
                        {
                            matrix[i][j] = matrix[i][k] + matrix[k][j];
                        }
                    }
                }
                // perform progress step for each line done
                progressBar1.PerformStep();
            }

            // for each two locations if transfer cost from one to the other is less then reversed, asign it to both
            for (int i = 1; i < locationN + 1; i++)
            {
                for (int j = 1; j < locationN + 1; j++)
                {
                    if (matrix[i][j] > matrix[j][i])
                    {
                        matrix[i][j] = matrix[j][i];
                    }
                    if (matrix[i][j] < matrix[j][i])
                    {
                        matrix[j][i] = matrix[i][j];
                    }
                }
                // perform progress step for each line done
                progressBar1.PerformStep();
            }

            // get construction index of curent matrix
                int matconstr = 0;
                // sum consts of all adjacent locations
                for (int i = 1; i < locationN; i++)
                {
                    matconstr = matconstr + matrix[i][i + 1];
                }

            label13.Text = Convert.ToString(matconstr);

            // return transformed generated matrix as function result
            return matrix;
        }


        // function to generate new random solution particle matrix (takes as input particle and object number - for dimensions, and location number for value limitation)
        public int[][] RandomParticleMatrix(int particleN, int objectN, int locationN)
        {
            // generate empty matrix of particleN times objectN dimension
            int[][] matrix = new int[particleN + 1][];
            for (int i = 1; i < particleN + 1; i++)
            {
                matrix[i] = new int[objectN + 1];
            }

            // for each element asign new random location in location range
            Random rnd = new Random();
            for (int i = 1; i < particleN + 1; i++)
            {
                for (int j = 1; j < objectN + 1; j++)
                {
                    matrix[i][j] = rnd.Next(1,locationN);
                }
            }
            // return generated result
            return matrix;
        }


        // function to generate new random solution inertion matrix (takes as input particle and object number - for dimensions, and maximum starting velocity for value limitation)
        public int[][] RandomInertioneMatrix(int particleN, int objectN, int maxV0)
        {
            // generate empty matrix
            int[][] matrix = new int[particleN + 1][];
            for (int i = 1; i < particleN + 1; i++)
            {
                matrix[i] = new int[objectN + 1];
            }

            // for each element asign new random number in maximum starting velocity range
            Random rnd = new Random();
            for (int i = 1; i < particleN + 1; i++)
            {
                for (int j = 1; j < objectN + 1; j++)
                {
                    int rndnmb = rnd.Next(-maxV0, maxV0);
                    if (rndnmb > -1) { rndnmb ++; }
                    matrix[i][j] = rndnmb;
                }
            }
            // return generated result
            return matrix;
        }


        // function to calculate p-median fitness of a solution matrix (takes solution matrix, particleN, locationN, objectN and cost matrix as input)
        public int[] MatrixFitnessMed(int[][] matrix, int particleN, int locationN, int objectN, int[][] costs)
        {
            // create fitness array (one value per particle)
            int[] fitness =  new int[particleN + 1];

            // create cost array (cost of transport to each particle)
            int[] cost = new int[objectN + 1];

            // set zeroth element of cost array to maximum (not to create confusion while calculation lowest in the array)
            cost[0] = 9999;


            // find the cost of transport between each location and object "closest" to it. Add it into a sum po get p-medain fitness.
            // Do this for each solution particle >>

                // for each particle
                for (int k = 1; k < particleN + 1; k++)
                {
                    // for each location
                    for (int i = 1; i < locationN + 1; i++)
                    {
                        // and particle element (representing placed object)
                        for (int j = 1; j < objectN + 1; j++)
                        {
                            // calculate cost between location and object location
                            cost[j] = costs[i][matrix[k][j]];
                        }
                        // add minimal value to fintess sum for that solution particle
                        fitness[k] = fitness[k] +cost.Min();
                    }
                }

            // set zeroth element of fitness array to maximum(not to create confusion while calculation lowest in the array)
            fitness[0] = 999999;

            // return p-median fitness of each particle in an array
            return fitness;
        }


        // function to calculate p-center fitness of a solution matrix (takes solution matrix, particleN, locationN, objectN and cost matrix as input)
        public int[] MatrixFitnessCent(int[][] matrix, int particleN, int locationN, int objectN, int[][] costs)
        {
            // create fitness array (one value per particle)
            int[] fitness = new int[particleN + 1];

            // create cost array (cost of transport to each particle)
            int[] cost = new int[objectN + 1];

            // set zeroth element of cost array to maximum (not to create confusion while calculation lowest in the array)
            cost[0] = 9999;

            // find the cost of transport between each location and object "closest" to it. Remember only higher value as a fitness value.
            // Do this for each solution particle >>

                // for each particle
                for (int k = 1; k < particleN + 1; k++)
                {
                    // for each location
                    for (int i = 1; i < locationN + 1; i++)
                    {
                        // and particle element (representing placed object)
                        for (int j = 1; j < objectN + 1; j++)
                        {
                            // calculate cost between location and object location
                            cost[j] = costs[i][matrix[k][j]];
                        }
                        // if cost is figher than current fitness save it as new fitness
                        if (cost.Min() > fitness[k]) {fitness[k] = cost.Min();}
                    }
                }

            // set zeroth element of fitness array to maximum(not to create confusion while calculation lowest in the array)
            fitness[0] = 999999;

            // return p-median fitness of each particle in an array
            return fitness;
        }


        // !!! function to produce new particle step (takes user defined prefernces as input)
        // returns a touple containing new particle position matrix and its new inertion matrix
        public Tuple<int[][], int[][]> step(int[][] pMatrix, int[][] lMatrix, int[] vbest, int[][] vMatrix, int[][] nvMatrix, int pN, int oN, double C0, double Cl, double Cg, int max, int lN)
        {
            // create new matrices that will contain results
            // particle matrix after iteration
            int[][] step = new int[pN+1][];
            // inertion matrix after iteration
            int[][] newM = new int[pN+1][];

            Random rnd = new Random();

            // for each particle
            for (int i = 1; i < pN+1; i++)
            {
                //initialize matrix lines
                step[i] = new int[oN+1];
                newM[i] = new int[oN+1];

                // for each dimension (object)
                for (int j = 1; j < oN+1; j++)
                {
                    // SET MOVEMENT TO SUM OF:
                    // 1. distance (and direction) to local best times its influence
                    double locmove = (lMatrix[i][j] - pMatrix[i][j]) *Cl;
                    if (locmove > max) { locmove = max; }
                    else if (locmove < -max) { locmove = -max; }

                    //2. distance (and direction) to global best times its influence
                    double globmove = (vbest[j] - pMatrix[i][j]) * Cg;
                    if (globmove > max) { globmove = max; }
                    else if (globmove < -max) { globmove = -max; }

                    //3. initial inertia in that dimension times its influence
                    double inermove = nvMatrix[i][j] * C0;
                    if (inermove > max) { inermove = max; }
                    else if (inermove < -max) { inermove = -max; }

                    // set movement to the sum (write into new inertia matrix defeind earlier)
                    step[i][j] = Convert.ToInt32(Math.Ceiling(locmove + globmove + inermove + rnd.Next(-max,max)));
                    if (step[i][j] == 0) { step[i][j] = -1; }

                    //if it tops maximum inertia, set to max
                    //if (move > max) { step[i][j] = max; }
                    //else if (move < -max) { step[i][j] = -max; }

                    // move the particle (write new position into new particle position matrix defeind earlier)
                    newM[i][j] = pMatrix[i][j] + step[i][j];

                    //if it crosses location maximum or miniumum, set to max (1 or lN)
                    if (newM[i][j] < 1) { newM[i][j] = 1; }
                    if (newM[i][j] > lN) { newM[i][j] = lN; }
                }
            }

            //return result tuple
            return Tuple.Create(newM,step);
        }


        // function that returns solution search log
        // Result is a file filled with particle postitions through iterations. Can be used to introspect their movement.
        public string Log(string log,int[][] pMatrix, int[][] nvMatrix, int[][] vMatrix, int[] fit, int pN, int oN)
        {
            //for each particle
            for (int u = 1; u < pN + 1; u++)
            {
                // and for each object in it
                for (int t = 1; t < oN + 1; t++)
                {
                    //write current location and comma
                    log = log + Convert.ToString(pMatrix[u][t]) + " ("; 
                    log = log + Convert.ToString(vMatrix[u][t]) + ",";
                    log = log + Convert.ToString(nvMatrix[u][t]) + "), ";
                }
                // add new line between particle
                log = log + " " + fit[u] + Environment.NewLine;
            }
            // and an extra one between iterations
            log = log + Environment.NewLine + Environment.NewLine;

            //return log as a string
            return log;
        }


        // function to get informtion on each iteration - used for introspection
        public int Solution(int c, long time, int best, int nbest, double fitavg)
        {
            // update iteration count field
            iter_out.Text = Convert.ToString(c);
            iter_out.Invalidate();
            iter_out.Update();

            // update elapsed time field
            time_out.Text = Convert.ToString(time / 1000);
            time_out.Invalidate();
            time_out.Update();

            // update best found fitness field
            fit_out.Text = Convert.ToString(best);
            fit_out.Invalidate();
            fit_out.Update();

            // update best fintess at current iteration field
            now_out.Text = Convert.ToString(nbest);
            now_out.Invalidate();
            now_out.Update();

            // update average fintess at current iteration field
            avg_out.Text = Convert.ToString(Math.Round(fitavg));
            avg_out.Invalidate();
            avg_out.Update();

            chart.Series["optimum"].Points.AddXY(c, opt);
            chart.Series["best_fit"].Points.AddXY(c, best);
            chart.Series["best_now"].Points.AddXY(c, nbest);
            chart.Series["avg_now"].Points.AddXY(c, Convert.ToInt32(Math.Round(fitavg)));

            chart.Invalidate();
            chart.Update();

            // out
            return 0;
        }
    }
}