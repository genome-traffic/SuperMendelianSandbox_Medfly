using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.ComponentModel;
using System.IO;


namespace DrivesMedfly
{
    class Simulation
    {
        public List<Organism> Adults = new List<Organism>();
        public List<Organism> Eggs = new List<Organism>();
        public static Random random = new Random();
      
        /*-------------------- Simulation Parameters ---------------------------------*/

        public int Generations = 20;
        public int Iterations = 100;

        public int PopulationCap = 500;
        public float Mortality = 0.1f;
        public int GlobalEggsPerFemale = 50;
        public int Sample = 47;

        public bool ApplyIntervention = true;
        public int StartingNumberOfWTFemales = 250;
        public int StartingNumberOfWTMales = 250;
        public int StartIntervention = 2;
        public int EndIntervention = 2;
        public int InterventionReleaseNumber = 125;

        string[] Track = {"TRA","FFER"};
        //string[] Track = { "ZPG" };

        /*------------------------------- The Simulation ---------------------------------------------*/

        public void Simulate()
        { 
            string pathdesktop = (string)Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            pathdesktop = pathdesktop + "/model";
            string pathString = System.IO.Path.Combine(pathdesktop, "Output_white_tra.csv");
            Console.WriteLine("Writing output to: " + pathString);
            File.Create(pathString).Dispose();

            Console.WriteLine("Simulation Starts.");

            using (var stream = File.OpenWrite(pathString))
            using (var Fwriter = new StreamWriter(stream))
            {
                // THE ACTUAL SIMULATION
                for (int cIterations = 1; cIterations <= Iterations; cIterations++)
                {
                    Console.WriteLine("Iteration " + cIterations + " out of " + Iterations);
                    Adults.Clear();
                    Eggs.Clear();

                    if (ApplyIntervention)
                        Populate_with_WT();
                    else
                        Populate_with_Setup();

                    Shuffle.ShuffleList(Adults);

                    for (int cGenerations = 1; cGenerations <= Generations; cGenerations++)
                    {
                        if (ApplyIntervention)
                        {
                            if ((cGenerations >= StartIntervention) && (cGenerations <= EndIntervention))
                            {
                                Intervention();
                            }
                        }
                        #region output data to file

                        //------------------------ Genotypes -------

                        List<string> Genotypes = new List<string>();

                         foreach (Organism O in Adults)
                         {
                             foreach (string s in Track)
                             {
                                Genotypes.Add(s + "," + O.GetGenotype(s));
                             }
                         }

                        var queryG = Genotypes.GroupBy(s => s)
                           .Select(g => new { Name = g.Key, Count = g.Count() });

                        foreach (var result in queryG)
                        {
                          Fwriter.WriteLine("{0},{1},{2},{3},all", cIterations, cGenerations, result.Name, result.Count);
                        }

                        Genotypes.Clear();

                        int cSample = Sample;
                        foreach (Organism O in Adults)
                        {
                            if (cSample > 0)
                            {
                                foreach (string s in Track)
                                {
                                    Genotypes.Add(s + "," + O.GetGenotype(s));
                                }
                                cSample--;
                            }
                        }

                        var queryGs = Genotypes.GroupBy(s => s)
                           .Select(g => new { Name = g.Key, Count = g.Count() });

                        foreach (var result in queryGs)
                        {
                            Fwriter.WriteLine("{0},{1},{2},{3},sample", cIterations, cGenerations, result.Name, result.Count);
                        }

                        //------------------------- Sex -----------
                        int numberofallmales = 0;
                        int numberofallfemales = 0;
                        foreach (Organism O in Adults)
                        {
                            if (O.GetSex() == "female")
                                numberofallfemales++;
                            else
                                numberofallmales++;
                        }
                        Fwriter.WriteLine("{0},{1},{2},{3},{4},{5},{6}", cIterations, cGenerations, "Sex", "Males", "NA", numberofallmales, "all");
                        Fwriter.WriteLine("{0},{1},{2},{3},{4},{5},{6}", cIterations, cGenerations, "Sex", "Females", "NA", numberofallfemales, "all");

                        //------------------------- Sex Karyotype -----------
                        int numberofXX = 0;
                        int numberofXY = 0;
                        foreach (Organism O in Adults)
                        {

                            switch (O.GetSexChromKaryo())
                            {
                                case "XX":
                                    {
                                        numberofXX++;
                                        break;
                                    }
                                case "XY":
                                    {
                                        numberofXY++;
                                        break;
                                    }
                                case "YX":
                                    {
                                        numberofXY++;
                                        break;
                                    }
                                default:
                                    {
                                        Console.WriteLine(O.GetSexChromKaryo() + " should not exist!");
                                        break;
                                    }
                            }

                        }
                        Fwriter.WriteLine("{0},{1},{2},{3},{4},{5},{6}", cIterations, cGenerations, "Sex_Karyotype", "XX", "NA", numberofXX, "all");
                        Fwriter.WriteLine("{0},{1},{2},{3},{4},{5},{6}", cIterations, cGenerations, "Sex_Karyotype", "XY", "NA", numberofXY, "all");



                        #endregion

                        Shuffle.ShuffleList(Adults);
                        CrossAll();
                        Adults.Clear();
                        Shuffle.ShuffleList(Eggs);

                        #region Return Adults from Eggs for the next Generation
                        int EggsToBeReturned = 0;

                        if (Eggs.Count <= PopulationCap)
                            EggsToBeReturned = Eggs.Count;
                        else
                            EggsToBeReturned = PopulationCap;

                        for (int na = 0; na < EggsToBeReturned; na++)
                        {
                            Adults.Add(new Organism(Eggs[na]));
                        }
                        #endregion

                        Eggs.Clear();

                    }

                }
                // END OF SIMULATION

                Fwriter.Flush();
            }
        }

        //---------------------- Define Organisms, Genotypes and Starting Populations -----------------------------------------------------

        public void Populate_with_WT()
        {
            for (int i = 0; i < StartingNumberOfWTFemales; i++)
            {
                Adults.Add(new Organism(GenerateWTFemale()));
            }
            for (int i = 0; i < StartingNumberOfWTMales; i++)
            {
                Adults.Add(new Organism(GenerateWTMale()));
            }
        }

        public void Populate_with_Setup()
        {
            for (int i = 0; i < 300; i++)
            {
                Adults.Add(new Organism(GenerateWTFemale()));
            }
            for (int i = 0; i < 120; i++)
            {
                Adults.Add(new Organism(GenerateWTMale()));
            }

            for (int i = 0; i < 60; i++)
            {
                Adults.Add(new Organism(Generate_DriveMale()));
            }
        }

        public void Intervention()
        {
            for (int i = 0; i < InterventionReleaseNumber; i++)
            {
                Adults.Add(new Organism(Generate_DriveMale()));
            }
        }

        public Organism GenerateWTFemale()
        {
            Organism WTFemale = new Organism();

            GeneLocus FFERa = new GeneLocus("FFER", 1, "WT");
            FFERa.Traits.Add("Conservation", 99);
            FFERa.Traits.Add("Hom_Repair", 95);
            GeneLocus FFERb = new GeneLocus("FFER", 1, "WT");
            FFERb.Traits.Add("Conservation", 99);
            FFERb.Traits.Add("Hom_Repair", 95);

            GeneLocus TRAa = new GeneLocus("TRA", 2, "WT");
            TRAa.Traits.Add("Conservation", 99);
            TRAa.Traits.Add("Hom_Repair", 95);
            GeneLocus TRAb = new GeneLocus("TRA", 2, "WT");
            TRAb.Traits.Add("Conservation",99);
            TRAb.Traits.Add("Hom_Repair", 95);

            Chromosome ChromXa = new Chromosome("X", "Sex");
            Chromosome ChromXb = new Chromosome("X", "Sex");
            Chromosome Chrom2a = new Chromosome("2", "2");
            Chromosome Chrom2b = new Chromosome("2", "2");
            Chromosome Chrom3a = new Chromosome("3", "3");
            Chromosome Chrom3b = new Chromosome("3", "3");

            Chrom2a.GeneLocusList.Add(FFERa);
            Chrom2b.GeneLocusList.Add(FFERb);
       
            Chrom3a.GeneLocusList.Add(TRAa);
            Chrom3b.GeneLocusList.Add(TRAb);

            WTFemale.ChromosomeListA.Add(ChromXa);
            WTFemale.ChromosomeListB.Add(ChromXb);
            WTFemale.ChromosomeListA.Add(Chrom2a);
            WTFemale.ChromosomeListB.Add(Chrom2b);
            WTFemale.ChromosomeListA.Add(Chrom3a);
            WTFemale.ChromosomeListB.Add(Chrom3b);

            WTFemale.MaternalTRA = true;

            return WTFemale;
        }

        public Organism GenerateWTMale()
        {
            Organism WTMale = new Organism(GenerateWTFemale());
            Chromosome ChromY = new Chromosome("Y", "Sex");
            GeneLocus MaleFactor = new GeneLocus("MaleDeterminingLocus", 1, "WT");
            ChromY.GeneLocusList.Add(MaleFactor);

            WTMale.ChromosomeListA[0] = ChromY;
      
            return WTMale;
        }

        public Organism Generate_DriveMale()
        {
            Organism FFD_Male = new Organism(GenerateWTMale());

            GeneLocus FFD = new GeneLocus("FFER", 1, "Construct");
            FFD.Traits.Add("transgene_Cas9", 95);
            FFD.Traits.Add("transgene_TRA", 1);
            FFD.Traits.Add("transgene_FFER", 1);
            FFD.Traits.Add("Hom_Repair", 95);

            Organism.ModifyAllele(ref FFD_Male.ChromosomeListA, FFD, "WT");
            return FFD_Male;           
        }

       

        //----------------------- Simulation methods ----------------------------------------------------


        public void PerformCross(Organism Dad, Organism Mum, ref List<Organism> EggList)
        {
            int EggsPerFemale = GlobalEggsPerFemale;

            EggsPerFemale = (int)(EggsPerFemale * Dad.GetFertility() * Mum.GetFertility());

                for (int i = 0; i < EggsPerFemale; i++)
                {
                    EggList.Add(new Organism(Dad,Mum));
                }
        }
        public void CrossAll()
        {

            int EffectivePopulation = (int)((1 - Mortality) * PopulationCap);
  
            int numb;
            foreach (Organism F1 in Adults)
            {
                if (F1.GetSex() == "male")
                {
                    continue;
                }
                else
                {
                    for (int a = 0; a < EffectivePopulation; a++)
                    {
                        numb = random.Next(0, Adults.Count);
                        if (Adults[numb].GetSex() == "male")
                        {
                            PerformCross(Adults[numb], F1, ref Eggs);
                            break;
                        }
                    }
                }

            }

        }

    }
}
