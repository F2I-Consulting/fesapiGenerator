/*-----------------------------------------------------------------------
Licensed to the Apache Software Foundation (ASF) under one
or more contributor license agreements.  See the NOTICE file
distributed with this work for additional information
regarding copyright ownership.  The ASF licenses this file
to you under the Apache License, Version 2.0 (the
"License"; you may not use this file except in compliance
with the License.  You may obtain a copy of the License at

  http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing,
software distributed under the License is distributed on an
"AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY
KIND, either express or implied.  See the License for the
specific language governing permissions and limitations
under the License.
-----------------------------------------------------------------------*/
using System;
using System.Collections.Generic;
using System.Threading;

namespace fesapiGenerator
{
    /// <summary>
    /// This is the main class of the fesapi generator addin.
    /// </summary>
    public class Main
    {
        #region constants

        /// <summary>
        /// Names of the addin menu options
        /// </summary>
        private const string menuHeader = "-&Fesapi Generator";
        private const string energisticsModelAlterationGenerateCode = "&Generate fesapi with Energistics model alteration";
        private const string fesapiModelGenerationMenuGenerateFesapiModel = "&Only generate fesapi model";
        private const string fesapiModelGenerationMenuUpdateFesapiModel = "&Only update fesapi model";
        private const string fesapiModelGenerationMenuGenerateCode = "&Only generate fesapi code from fesapi generated model";
        private const string energisticsModelAlterationMenuUpdateEnergisticsModel = "&Only update Energistics model";
        private const string energisticsModelAlterationMenuTransformEnergisticsModel = "&Only transform Energistics model into C++ model";
        private const string energisticsModelAlterationMenuGenerateCode = "&Only generate fesapi code from Energistics model alteration";
        
        #endregion

        #region members

        /// <summary>
        /// The Repository is the main container of all structures such as models, Packages and elements.
        /// </summary>
        private EA.Repository repository;

        #endregion

        #region EA addin interface implementation

        /// <summary>
        /// Called Before EA starts to check Add-In Exists
        /// Nothing is done here.
        /// This operation needs to exists for the addin to work
        /// </summary>
        /// <param name="Repository">the EA repository</param>
        /// <returns>a string</returns>
        public String EA_Connect(EA.Repository repository)
        {
            //No special processing required.
            return "a string";
        }

        /// <summary>
        /// EA calls this operation when it exists. Can be used to do some cleanup
        /// </summary>
        public void EA_Disconnect()
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        /// <summary>
        /// Called when user Clicks Add-Ins Menu item from within EA.
        /// Populates the Menu with our desired selections.
        /// Location can be "TreeView" "MainMenu" or "Diagram".
        /// </summary>
        /// <param name="repository">the repository</param>
        /// <param name="location">the location of the menu</param>
        /// <param name="menuName">the name of the menu</param>
        /// <returns></returns>
        public object EA_GetMenuItems(EA.Repository repository, string location, string menuName)
        {
            switch (menuName)
            {
                // defines the top level menu option
                case "":
                    return menuHeader;
                // defines the submenu options
                case menuHeader:
                    string[] subMenus = { energisticsModelAlterationGenerateCode, "-&Fesapi model generation methodology", "-&Energistics model alteration methodology" };
                    return subMenus;
                case "-&Fesapi model generation methodology":
                    string[] fesapiModelGenerationMenus = { fesapiModelGenerationMenuGenerateFesapiModel, fesapiModelGenerationMenuUpdateFesapiModel, fesapiModelGenerationMenuGenerateCode };
                    return fesapiModelGenerationMenus;
                case "-&Energistics model alteration methodology":
                    string[] energisticsModelAlterationMenus = { energisticsModelAlterationMenuUpdateEnergisticsModel, energisticsModelAlterationMenuTransformEnergisticsModel, energisticsModelAlterationMenuGenerateCode };
                    return energisticsModelAlterationMenus;
            }
            return "";
        }

        /// <summary>
        /// returns true if a project is currently opened
        /// </summary>
        /// <param name="repository">the repository</param>
        /// <returns>true if a project is opened in EA</returns>
        bool IsProjectOpen(EA.Repository repository)
        {
            try
            {
                // try to get some models in order to make sure a project is currently open
                EA.Collection c = repository.Models;

                this.repository = repository;

                // creation of the fesapi generator addin output tab for being able to log 
                // pieces of informations in it
                repository.CreateOutputTab(Constants.outputTabName);

                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Called once Menu has been opened to see what menu items should active.
        /// </summary>
        /// <param name="repository">the repository</param>
        /// <param name="location">the location of the menu</param>
        /// <param name="menuName">the name of the menu</param>
        /// <param name="itemName">the name of the menu item</param>
        /// <param name="isEnabled">boolean indicating whethe the menu item is enabled</param>
        /// <param name="IsChecked">boolean indicating whether the menu is checked</param>
        public void EA_GetMenuState(EA.Repository repository, string location, string menuName, 
            string itemName, ref bool isEnabled, ref bool IsChecked)
        {
            if (IsProjectOpen(repository))
            {
                switch (itemName)
                {
                    // define the state of the "Generate fesapi" option
                    case energisticsModelAlterationGenerateCode:
                        isEnabled = true;
                        break;
                    // define the state of the "Only generate fesapi model" option
                    case fesapiModelGenerationMenuGenerateFesapiModel:
                        isEnabled = true;
                        break;
                    // define the state of the "Only update fesapi model" option
                    case fesapiModelGenerationMenuUpdateFesapiModel:
                        isEnabled = true;
                        break;
                    // define the state of the "Only generate fesapi code" option (from fesapi model generation)
                    case fesapiModelGenerationMenuGenerateCode:
                        isEnabled = true;
                        break;
                    // define the state of the "Only update Energistics model" option
                    case energisticsModelAlterationMenuUpdateEnergisticsModel:
                        isEnabled = true;
                        break;
                    // define the state of the "Only transform Energistics model into C++ Model" option
                    case energisticsModelAlterationMenuTransformEnergisticsModel:
                        isEnabled = true;
                        break;
                    // define the state of the "Only generate fesapi code" option (from Energistics model alteration)
                    case energisticsModelAlterationMenuGenerateCode:
                        isEnabled = true;
                        break;
                    default:
                        isEnabled = false;
                        break;
                }
            }
            else
            {
                isEnabled = false;
            }
        }

        /// <summary>
        /// Called when user makes a selection in the menu.
        /// This is your main exit point to the rest of your Add-in
        /// </summary>
        /// <param name="repository">the repository</param>
        /// <param name="location">the location of the menu</param>
        /// <param name="menuName">the name of the menu</param>
        /// <param name="itemName">the name of the selected menu item</param> 
        public void EA_MenuClick(EA.Repository repository, string location, string menuName, string itemName)
        {
            switch (itemName)
            {
                // user has clicked the "Generate fesapi" option
                case energisticsModelAlterationGenerateCode:
                    this.generateFesapiOption();
                    break;
                // user has clicked the "Only generate fesapi model" option
                case fesapiModelGenerationMenuGenerateFesapiModel:
                    this.onlyGenerateFesapiModelOption();
                    break;
                // user has clicked the "Only update fesapi model" option
                case fesapiModelGenerationMenuUpdateFesapiModel:
                    this.onlyUpdateFesapiModelOption();
                    break;
                // user has clicked the "Only generate fesapi code from fesapi generated model" option
                case fesapiModelGenerationMenuGenerateCode:
                    this.onlygGenerateCodeFromFesapiModelGenerationOption();
                    break;
                // user has clicked the "Only update Energistics model" option
                case energisticsModelAlterationMenuUpdateEnergisticsModel:
                    this.onlyUpdateEnergisticsModelOption();
                    break;
                // user has clicked the "Only transform Energistics model into C++ Model" option
                case energisticsModelAlterationMenuTransformEnergisticsModel:
                    this.onlyTansformEnergisticsModelToCppModelOption();
                    break;
                // user has clicked the "Only generate fesapi code" option
                case energisticsModelAlterationMenuGenerateCode:
                    this.onlyGenerateCodeFromEnergisticsModelAlterationOption();
                    break;
            }
        }

        #endregion

        #region Menu options behaviors

        /// <summary>
        /// Called when the user clicks the "Generate fesapi" option
        /// </summary>
        private void generateFesapiOption()
        {
            // make shure the Fesapi Generator tab is visible to the user
            repository.EnsureOutputVisible(Constants.outputTabName);

            // make sure the models are up to date
            repository.Models.Refresh();

            // getting the common, resqml and fesapi models with common/v2.0 and resqml/v2.0.1 packages
            EA.Package commonModel;
            EA.Package commonV2Package;
            EA.Package resqmlModel;
            EA.Package resqmlV2_0_1Package;
            EA.Package fesapiModel;
            try
            {
                commonModel = repository.Models.GetByName(Constants.commonModelName);
                commonV2Package = commonModel.Packages.GetByName(Constants.commonV2PackageName);
                resqmlModel = repository.Models.GetByName(Constants.resqmlModelName);
                resqmlV2_0_1Package = resqmlModel.Packages.GetByName(Constants.resqmlV2_0_1PackageName);
                fesapiModel = repository.Models.GetByName(Constants.fesapiModelName);
            }
            catch (Exception)
            {
                Tool.showMessageBox(repository, "The project must carry common, resqml and fesapi models with common/v2.0 and resqml/v2.0.1 packages!");
                return;
            }

            // getting the outputPath from the user

            string outputPath = Tool.getOutputPath();
            if (outputPath == "")
                return;

            // since subsequent operations are time consuming, we need to set tup
            // a progress bar
            ProgressDialog progressDialog = new ProgressDialog();
            Thread backgroundThread = new Thread(
                new ThreadStart(() =>
                {
                    updateEnergisticsModel(commonModel, commonV2Package, resqmlModel, resqmlV2_0_1Package, fesapiModel);

                    // getting the common/updated v2.0 and resqml/updated v2.0.1 packages
                    EA.Package updatedCommonV2Package;
                    EA.Package updatedResqmlV2_0_1Package;
                    try
                    {
                        updatedCommonV2Package = commonModel.Packages.GetByName(Constants.updatedCommonV2PackageName);
                        updatedResqmlV2_0_1Package = resqmlModel.Packages.GetByName(Constants.updatedResqmlV2_0_1PackageName);
                    }
                    catch (Exception)
                    {
                        Tool.showMessageBox(repository, "An error occured during the Energistics model update since no common/updated v2.0 or resqml/updated v2.0.1 package exists!");
                        return;
                    }

                    transformEnergisticsModelToCppModel(updatedCommonV2Package, updatedResqmlV2_0_1Package);

                    // closing progress bar dialog
                    progressDialog.BeginInvoke(new Action(() => progressDialog.Close()));
                }));
            backgroundThread.Start();
            progressDialog.ShowDialog();

            // make sure the model view is up to date in the Enterprise Architect GUI
            repository.RefreshModelView(0);

            // make sure the models are up to date
            repository.Models.Refresh();

            // looking for an existing c++ energistics model
            EA.Package cppEnergisticsModel;
            try
            {
                cppEnergisticsModel = repository.Models.GetByName(Constants.cppEnergisticsModelName);
            }
            catch (Exception)
            {
                Tool.showMessageBox(repository, "An error occured during the Energistics model transformation since no " + Constants.cppEnergisticsModelName + " model exists!");
                return;
            }

            generateCodeFromEnergisticsModelAlteration(cppEnergisticsModel, outputPath);
        }

        /// <summary>
        /// Called when the user clicks the "Only generate fesapi model" option
        /// </summary>
        private void onlyGenerateFesapiModelOption()
        {
            // make shure the Fesapi Generator tab is visible to the user
            repository.EnsureOutputVisible(Constants.outputTabName);

            // make sure the models are up to date
            repository.Models.Refresh();

            // getting the common and resqml models with common/v2.0, common/v2.2, resqml/v2.2 and resqml/v2.0.1 packages
            EA.Package commonModel;
            EA.Package commonV2Package;
            EA.Package commonV2_2Package;
            EA.Package resqmlModel;
            EA.Package resqmlV2_0_1Package;
            EA.Package resqmlV2_2Package;
            try
            {
                commonModel = repository.Models.GetByName(Constants.commonModelName);
                commonV2Package = commonModel.Packages.GetByName(Constants.commonV2PackageName);
                commonV2_2Package = commonModel.Packages.GetByName(Constants.commonV2_2PackageName);
                resqmlModel = repository.Models.GetByName(Constants.resqmlModelName);
                resqmlV2_0_1Package = resqmlModel.Packages.GetByName(Constants.resqmlV2_0_1PackageName);
                resqmlV2_2Package = resqmlModel.Packages.GetByName(Constants.resqmlV2_2PackageName);
            }
            catch (Exception)
            {
                Tool.showMessageBox(repository, "The project must carry common, resqml and fesapi models with common/v2.0 and resqml/v2.0.1 packages!");
                return;
            }

            // since subsequent operations are time consuming, we need to set tup
            // a progress bar
            ProgressDialog progressDialog = new ProgressDialog();
            Thread backgroundThread = new Thread(
                new ThreadStart(() =>
                {
                    generateFesapiModel(commonModel, commonV2Package, commonV2_2Package, resqmlModel, resqmlV2_0_1Package, resqmlV2_2Package);

                    // closing progress bar dialog
                    progressDialog.BeginInvoke(new Action(() => progressDialog.Close()));
                }));
            backgroundThread.Start();
            progressDialog.ShowDialog();

            // make sure the model view is up to date in the Enterprise Architect GUI
            repository.RefreshModelView(0);
        }

        /// <summary>
        /// Called when the user clicks the "Only update fesapi model" option
        /// </summary>
        private void onlyUpdateFesapiModelOption()
        {
            // make shure the Fesapi Generator tab is visible to the user
            repository.EnsureOutputVisible(Constants.outputTabName);

            // make sure the models are up to date
            repository.Models.Refresh();

            // getting the 
            EA.Package fesapiModel;
            EA.Package fesapiCommonPackage;
            EA.Package fesapiResqml2Package;
            EA.Package fesapiResqml2_0_1Package;
            EA.Package fesapiResqml2_2Package;
            EA.Package customFesapiModel;
            EA.Package customFesapiCommonPackage;
            EA.Package customFesapiResqml2Package;
            EA.Package customFesapiResqml2_0_1Package;
            EA.Package customFesapiResqml2_2Package;
            try
            {
                fesapiModel = repository.Models.GetByName(Constants.fesapiModelName);
                EA.Package fesapiClassModel = fesapiModel.Packages.GetByName(Constants.fesapiClassModelName);
                fesapiCommonPackage = fesapiClassModel.Packages.GetByName(Constants.fesapiCommonPackageName);
                fesapiResqml2Package = fesapiClassModel.Packages.GetByName(Constants.fesapiResqml2PackageName);
                fesapiResqml2_0_1Package = fesapiClassModel.Packages.GetByName(Constants.fesapiResqml2_0_1PackageName);
                fesapiResqml2_2Package = fesapiClassModel.Packages.GetByName(Constants.fesapiResqml2_2PackageName);
                customFesapiModel = repository.Models.GetByName(Constants.customFesapiModelName);
                EA.Package customFesapiClassModel = customFesapiModel.Packages.GetByName(Constants.customFesapiClassModelName);
                customFesapiCommonPackage = customFesapiClassModel.Packages.GetByName(Constants.customFesapiCommonPackageName);
                customFesapiResqml2Package = customFesapiClassModel.Packages.GetByName(Constants.customFesapiResqml2PackageName);
                customFesapiResqml2_0_1Package = customFesapiClassModel.Packages.GetByName(Constants.customFesapiResqml2_0_1PackageName);
                customFesapiResqml2_2Package = customFesapiClassModel.Packages.GetByName(Constants.customFesapiResqml2_2PackageName);
            }
            catch (Exception)
            {
                Tool.showMessageBox(repository, "The project must carry \"fesapi\" and \"custom fesapi\" models containing a \"Class Model\" with sub \"common\", \"resqml2\", \"resqml2_0_1\" and \"resqml2_2\" packages!");
                return;
            }

            // since subsequent operations are time consuming, we need to set tup
            // a progress bar
            ProgressDialog progressDialog = new ProgressDialog();
            Thread backgroundThread = new Thread(
                new ThreadStart(() =>
                {
                    updateFesapiModel(fesapiModel, fesapiCommonPackage, fesapiResqml2Package, fesapiResqml2_0_1Package, fesapiResqml2_2Package,
                        customFesapiModel, customFesapiCommonPackage, customFesapiResqml2Package, customFesapiResqml2_0_1Package, customFesapiResqml2_2Package);

                    // closing progress bar dialog
                    progressDialog.BeginInvoke(new Action(() => progressDialog.Close()));
                }));
            backgroundThread.Start();
            progressDialog.ShowDialog();

            // make sure the model view is up to date in the Enterprise Architect GUI
            repository.RefreshModelView(0);
        }

        /// <summary>
        /// Called when the user clicks the "Only generate fesapi code from fesapi generated model" option
        /// </summary>
        private void onlygGenerateCodeFromFesapiModelGenerationOption()
        {
            // make shure the Fesapi Generator tab is visible to the user
            repository.EnsureOutputVisible(Constants.outputTabName);

            // make sure the models are up to date
            repository.Models.Refresh();

            // looking for an existing fesapi model
            EA.Package fesapiModel;
            try
            {
                fesapiModel = repository.Models.GetByName(Constants.fesapiModelName);
            }
            catch (Exception)
            {
                Tool.showMessageBox(repository, "The project must carry a " + Constants.fesapiModelName + " model!");
                return;
            }

            // getting the outputPath from the user
            string outputPath = Tool.getOutputPath();
            if (outputPath != "")
            {
                generateCodeFromFesapiModelGeneration(fesapiModel, outputPath);
            }
        }

        /// <summary>
        /// Called when the user clicks the "Only update Energistics model" option
        /// </summary>
        private void onlyUpdateEnergisticsModelOption()
        {
            // make shure the Fesapi Generator tab is visible to the user
            repository.EnsureOutputVisible(Constants.outputTabName);

            // make sure the models are up to date
            repository.Models.Refresh();

            // getting the common, resqml and fesapi models with common/v2.0 and resqml/v2.0.1 packages
            EA.Package commonModel;
            EA.Package commonV2Package;
            EA.Package resqmlModel;
            EA.Package resqmlV2_0_1Package;
            EA.Package fesapiModel;
            try
            {
                commonModel = repository.Models.GetByName(Constants.commonModelName);
                commonV2Package = commonModel.Packages.GetByName(Constants.commonV2PackageName);
                resqmlModel = repository.Models.GetByName(Constants.resqmlModelName);
                resqmlV2_0_1Package = resqmlModel.Packages.GetByName(Constants.resqmlV2_0_1PackageName);
                fesapiModel = repository.Models.GetByName(Constants.fesapiModelName);
            }
            catch (Exception)
            {
                Tool.showMessageBox(repository, "The project must carry common, resqml and fesapi models with common/v2.0 and resqml/v2.0.1 packages!");
                return;
            }

            // since subsequent operations are time consuming, we need to set tup
            // a progress bar
            ProgressDialog progressDialog = new ProgressDialog();
            Thread backgroundThread = new Thread(
                new ThreadStart(() =>
                {
                    updateEnergisticsModel(commonModel, commonV2Package, resqmlModel, resqmlV2_0_1Package, fesapiModel);

                    // closing progress bar dialog
                    progressDialog.BeginInvoke(new Action(() => progressDialog.Close()));
                }));
            backgroundThread.Start();
            progressDialog.ShowDialog();

            // make sure the model view is up to date in the Enterprise Architect GUI
            repository.RefreshModelView(0);
        }

        /// <summary>
        /// Called when the user clicks the "Only transform Energistics model into C++ Model" option
        /// </summary>
        private void onlyTansformEnergisticsModelToCppModelOption()
        {
            // make shure the Fesapi Generator tab is visible to the user
            repository.EnsureOutputVisible(Constants.outputTabName);

            // make sure the models are up to date
            repository.Models.Refresh();

            // getting the common, resqml and fesapi models with common/updated v2.0 and resqml/updated v2.0.1 packages
            EA.Package commonModel;
            EA.Package updatedCommonV2Package;
            EA.Package resqmlModel;
            EA.Package updatedResqmlV2_0_1Package;
            EA.Package fesapiModel;
            try
            {
                commonModel = repository.Models.GetByName(Constants.commonModelName);
                updatedCommonV2Package = commonModel.Packages.GetByName(Constants.updatedCommonV2PackageName);
                resqmlModel = repository.Models.GetByName(Constants.resqmlModelName);
                updatedResqmlV2_0_1Package = resqmlModel.Packages.GetByName(Constants.updatedResqmlV2_0_1PackageName);
                fesapiModel = repository.Models.GetByName(Constants.fesapiModelName);
            }
            catch (Exception)
            {
                Tool.showMessageBox(repository, "The project must carry common, resqml and fesapi models with common/updated v2.0 and resqml/updated v2.0.1 packages!");
                return;
            }

            // since subsequent operations are time consuming, we need to set tup
            // a progress bar
            ProgressDialog progressDialog = new ProgressDialog();
            Thread backgroundThread = new Thread(
                new ThreadStart(() =>
                {
                    transformEnergisticsModelToCppModel(updatedCommonV2Package, updatedResqmlV2_0_1Package);

                    // closing progress bar dialog
                    progressDialog.BeginInvoke(new Action(() => progressDialog.Close()));
                }));
            backgroundThread.Start();
            progressDialog.ShowDialog();

            // make sure the model view is up to date in the Enterprise Architect GUI
            repository.RefreshModelView(0);
        }

        /// <summary>
        /// Called when the user clicks the "Only generate fesapi code" option
        /// </summary>
        private void onlyGenerateCodeFromEnergisticsModelAlterationOption()
        {
            // make shure the Fesapi Generator tab is visible to the user
            repository.EnsureOutputVisible(Constants.outputTabName);

            // make sure the models are up to date
            repository.Models.Refresh();

            // looking for an existing c++ energistics model
            EA.Package cppEnergisticsModel;
            try
            {
                cppEnergisticsModel = repository.Models.GetByName(Constants.cppEnergisticsModelName);
            }
            catch (Exception)
            {
                Tool.showMessageBox(repository, "The project must carry a " + Constants.cppEnergisticsModelName + " model!");
                return;
            }

            // getting the outputPath from the user
            string outputPath = Tool.getOutputPath();
            if (outputPath != "")
            {
                generateCodeFromEnergisticsModelAlteration(cppEnergisticsModel, outputPath);
            }
        }

        #endregion

        #region private methods

        /// <summary>
        /// Generate the fesapi model according to input common and resqml models.
        /// </summary>
        /// <param name="commonModel">Input common model</param>
        /// <param name="commonV2Package">Input common/v2.0 package</param>
        /// <param name="commonV2_2Package">Input common/v2.2 package</param>
        /// <param name="resqmlModel">Input resqml model</param>
        /// <param name="resqmlV2_0_1Package">Input resqml/v2.0.1 package</param>
        /// <param name="resqmlV2_2Package">Input resqml/v2.2 package</param>
        private void generateFesapiModel(
            EA.Package commonModel, 
            EA.Package commonV2Package, 
            EA.Package commonV2_2Package, 
            EA.Package resqmlModel, 
            EA.Package resqmlV2_0_1Package, 
            EA.Package resqmlV2_2Package)
        {
            Tool.log(repository, "Generate fesapi model (" + DateTime.Now.ToString() + ")...");

            // looking for an existing fesapi model
            EA.Package fesapiModel;
            try
            {
                fesapiModel = repository.Models.GetByName(Constants.fesapiModelName);
            }
            catch  (Exception)
            {
                fesapiModel = null;
            }

            // if such a fesapi model already exists we delete it
            if (fesapiModel != null)
            {
                // looking for the index of fesapi
                short fesapiModelIndex = Tool.getIndex(repository, fesapiModel);

                // removing existing fesapi model
                repository.Models.Delete(fesapiModelIndex);
            }

            // creating a fesapi model with packages
            fesapiModel = repository.Models.AddNew(Constants.fesapiModelName, "");
            if (!(fesapiModel.Update()))
            {
                Tool.showMessageBox(repository, fesapiModel.GetLastError());
                return;
            }

            EA.Package fesapiClassModel = fesapiModel.Packages.AddNew(Constants.fesapiClassModelName, "");
            if (!(fesapiClassModel.Update()))
            {
                Tool.showMessageBox(repository, fesapiClassModel.GetLastError());
                return;
            }

            EA.Package fesapiCommonPackage = fesapiClassModel.Packages.AddNew(Constants.fesapiCommonPackageName, "");
            if (!(fesapiCommonPackage.Update()))
            {
                Tool.showMessageBox(repository, fesapiCommonPackage.GetLastError());
                return;
            }

            EA.Package fesapiResqml2Package = fesapiClassModel.Packages.AddNew(Constants.fesapiResqml2PackageName, "");
            if (!(fesapiResqml2Package.Update()))
            {
                Tool.showMessageBox(repository, fesapiResqml2Package.GetLastError());
                return;
            }

            EA.Package fesapiResqml2_0_1Package = fesapiClassModel.Packages.AddNew(Constants.fesapiResqml2_0_1PackageName, "");
            if (!(fesapiResqml2_0_1Package.Update()))
            {
                Tool.showMessageBox(repository, fesapiResqml2_0_1Package.GetLastError());
                return;
            }

            EA.Package fesapiResqml2_2Package = fesapiClassModel.Packages.AddNew(Constants.fesapiResqml2_2PackageName, "");
            if (!(fesapiResqml2_2Package.Update()))
            {
                Tool.showMessageBox(repository, fesapiResqml2_2Package.GetLastError());
                return;
            }

            fesapiClassModel.Packages.Refresh();
            fesapiModel.Packages.Refresh();
            repository.Models.Refresh();

            FesapiModelGenerator fesapiModelGenerator = new FesapiModelGenerator(repository, commonModel, commonV2Package, commonV2_2Package, resqmlModel, resqmlV2_0_1Package, resqmlV2_2Package, fesapiModel, fesapiCommonPackage, fesapiResqml2Package, fesapiResqml2_0_1Package, fesapiResqml2_2Package);
            fesapiModelGenerator.generateFesapiModel();

            Tool.log(repository, "fesapi model generated (" + DateTime.Now.ToString() + ").");
        }

        private void updateFesapiModel(
            EA.Package fesapiModel, 
            EA.Package fesapiCommonPackage, 
            EA.Package fesapiResqml2Package, 
            EA.Package fesapiResqml2_0_1Package, 
            EA.Package fesapiResqml2_2Package,
            EA.Package customFesapiModel, 
            EA.Package customFesapiCommonPackage, 
            EA.Package customFesapiResqml2Package, 
            EA.Package customFesapiResqml2_0_1Package, 
            EA.Package customFesapiResqml2_2Package)
        {
            Tool.log(repository, "Updating fesapi model (" + DateTime.Now.ToString() + ")...");

            FesapiModelUpdate fesapiModelUpdate = new FesapiModelUpdate(repository, fesapiModel, fesapiCommonPackage, fesapiResqml2Package, fesapiResqml2_0_1Package, fesapiResqml2_2Package,
                customFesapiModel, customFesapiCommonPackage, customFesapiResqml2Package, customFesapiResqml2_0_1Package, customFesapiResqml2_2Package);
            fesapiModelUpdate.updateFesapiModel();

            Tool.log(repository, "fesapi model updated (" + DateTime.Now.ToString() + ").");
        }

        private void generateCodeFromFesapiModelGeneration(EA.Package fesapiModel, string outputPath)
        {
            Tool.log(repository, "Generating fesapi code from fesapi model generation (" + DateTime.Now.ToString() + ")...");

            // getting the list of classes to generate
            List<EA.Element> toGenerateClassList = new List<EA.Element>();
            Tool.fillElementList(fesapiModel, toGenerateClassList);

            // setting the code output path for each class
            EA.Project project = repository.GetProjectInterface();
            foreach (EA.Element c in toGenerateClassList)
            {
                c.Genfile = outputPath + "\\" + repository.GetPackageByID(c.PackageID).Name + "\\" + c.Name + ".h";
                if (!(c.Update()))
                {
                    Tool.showMessageBox(repository, c.GetLastError());
                    continue;
                }
            }

            // generating the code
            project.GeneratePackage(fesapiModel.PackageGUID, "recurse=1;overwrite=1");

            Tool.log(repository, "fesapi code generated (" + DateTime.Now.ToString() + ").");
        }

        /// <summary>
        /// Updated the energistics model (common and resqml model) according to the input fesapi model.
        /// It is required to process this update before transforming the Energistics model into a
        /// C++ Energistics model. This is basically the first step of the fesapi generation
        /// </summary>
        /// <param name="commonModel">Input common model</param>
        /// <param name="commonV2Package">Input common/v2.0 package</param>
        /// <param name="resqmlModel">Input resqml model</param>
        /// <param name="resqmlV2_0_1Package">Input resqml/v2.0.1 package</param>
        /// <param name="fesapiModel">Input fesapi model</param>
        private void updateEnergisticsModel(
            EA.Package commonModel,
            EA.Package commonV2Package,
            EA.Package resqmlModel,
            EA.Package resqmlV2_0_1Package,
            EA.Package fesapiModel)
        {
            Tool.log(repository, "Updating Energistics model...");

            // looking for common/updated v2.0 package
            EA.Package updatedCommonV2Package;
            try
            {
                updatedCommonV2Package = commonModel.Packages.GetByName(Constants.updatedCommonV2PackageName);
            }
            catch (Exception)
            {
                updatedCommonV2Package = null;
            }

            // if such a package exists we delete it
            if (updatedCommonV2Package != null)
            {
                // looking for the index of the common/updated v2.0 package
                short updatedCommonV2PackageIndex = Tool.getIndex(commonModel, updatedCommonV2Package);

                // removing existing common/updated v2.0 package
                commonModel.Packages.Delete(updatedCommonV2PackageIndex);
            }

            // creating a new common/updated v2.0 package
            EA.Project project = repository.GetProjectInterface();
            string xmiPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Constants.tempXmiPath;
            project.ExportPackageXMI(commonV2Package.PackageGUID, EA.EnumXMIType.xmiEADefault, 0, -1, 0, 0, xmiPath);

            project.ImportPackageXMI(commonModel.PackageGUID, xmiPath, 0, 1);
            System.IO.File.Delete(xmiPath);
            commonModel.Packages.Refresh();

            string commonV2PackageGUID = commonV2Package.PackageGUID;
            foreach (EA.Package p in commonModel.Packages)
            {
                if (p.Name == Constants.commonV2PackageName && p.PackageGUID != commonV2PackageGUID)
                {
                    updatedCommonV2Package = p;
                    break;
                }
            }

            updatedCommonV2Package.Name = Constants.updatedCommonV2PackageName;
            if (!(updatedCommonV2Package.Update()))
            {
                Tool.showMessageBox(repository, updatedCommonV2Package.GetLastError());
                return;
            }

            // looking for resqml/updated v2.0.1 packages
            EA.Package updatedResqmlV2_0_1Package;
            try
            {
                updatedResqmlV2_0_1Package = resqmlModel.Packages.GetByName(Constants.updatedResqmlV2_0_1PackageName);
            }
            catch (Exception)
            {
                updatedResqmlV2_0_1Package = null;
            }

            // if such a package exist, we overwrite them
            if (updatedResqmlV2_0_1Package != null)
            {
                // looking for the index of the resqml/updated v2.0.1 package
                short updatedResqmlV2_0_1PackageIndex = Tool.getIndex(resqmlModel, updatedResqmlV2_0_1Package);

                // removing existing resqml/updated v2.0.1 package
                resqmlModel.Packages.Delete(updatedResqmlV2_0_1PackageIndex);
            }

            // creating a new resqml/updated v2.0.1 package
            project.ExportPackageXMI(resqmlV2_0_1Package.PackageGUID, EA.EnumXMIType.xmiEADefault, 0, -1, 0, 0, xmiPath);

            project.ImportPackageXMI(resqmlModel.PackageGUID, xmiPath, 0, 1);
            System.IO.File.Delete(xmiPath);
            resqmlModel.Packages.Refresh();

            string resqmlV2_0_1PackageGUID = resqmlV2_0_1Package.PackageGUID;
            foreach (EA.Package p in resqmlModel.Packages)
            {
                if (p.Name == Constants.resqmlV2_0_1PackageName && p.PackageGUID != resqmlV2_0_1PackageGUID)
                {
                    updatedResqmlV2_0_1Package = p;
                    break;
                }
            }

            updatedResqmlV2_0_1Package.Name = Constants.updatedResqmlV2_0_1PackageName;
            if (!(updatedResqmlV2_0_1Package.Update()))
            {
                Tool.showMessageBox(repository, updatedResqmlV2_0_1Package.GetLastError());
                return;
            }

            EnergisticsModelUpdate energisticsModelUpdate = new EnergisticsModelUpdate(repository, updatedCommonV2Package, updatedResqmlV2_0_1Package, fesapiModel);
            energisticsModelUpdate.updateEnergisticsModel();

            Tool.log(repository, "Energistics model updated.");
        }

        /// <summary>
        /// Transforms an updated Energistics model (cf. updateEnergisticsModel method) into
        /// a C++ energistics model. This is basically the second step of the fesapi generation.
        /// This transformation relies on the "C++ Fesapi" model transformation template
        /// </summary>
        /// <param name="updatedCommonV2Package">Input updated common model</param>
        /// <param name="updatedResqmlV2_0_1Package">Input updated resqml model</param>
        private void transformEnergisticsModelToCppModel(
            EA.Package updatedCommonV2Package,
            EA.Package updatedResqmlV2_0_1Package)
        {
            Tool.log(repository, "Transforming Energistics model into a C++ model...");

            // looking for an existing c++ energistics model
            bool existCppEnergisticsModel;
            EA.Package cppEnergisticsModel = null;
            try
            {
                cppEnergisticsModel = repository.Models.GetByName(Constants.cppEnergisticsModelName);
                existCppEnergisticsModel = true;
            }
            catch (Exception)
            {
                existCppEnergisticsModel = false;
            }

            if (existCppEnergisticsModel)
            {
                // looking for the index of the C++ Energistics model package
                short transformedModelIndex = -1;
                for (short i = 0; i < repository.Models.Count; ++i)
                {
                    if (repository.Models.GetAt(i).PackageGUID == cppEnergisticsModel.PackageGUID)
                    {
                        transformedModelIndex = i;
                        break;
                    }
                }

                // removing existing C++ Energistics model
                repository.Models.Delete(transformedModelIndex);
            }

            // here, no C++ Energistics model exists or the user agree to overwrite it
            // in both case, we need to create a new one
            cppEnergisticsModel = repository.Models.AddNew(Constants.cppEnergisticsModelName, "");
            if (!(cppEnergisticsModel.Update()))
            {
                Tool.showMessageBox(repository, cppEnergisticsModel.GetLastError());
                return;
            }

            // creating a package in order to carry the C++ classes
            EA.Package cppEnergisticsPackage = cppEnergisticsModel.Packages.AddNew("Class Model", "");
            if (!(cppEnergisticsPackage.Update()))
            {
                Tool.showMessageBox(repository, cppEnergisticsPackage.GetLastError());
                return;
            }

            // collecting the Energistics classes to transform
            List<EA.Element> toTransformClassList = new List<EA.Element>();
            Tool.fillFesapiGenerationTaggedElementList(updatedCommonV2Package, toTransformClassList);
            Tool.fillFesapiGenerationTaggedElementList(updatedResqmlV2_0_1Package, toTransformClassList);

            // apply transformation
            EA.Project project = repository.GetProjectInterface();
            foreach (EA.Element c in toTransformClassList)
                project.TransformElement("C++ Fesapi", c.ElementGUID, cppEnergisticsPackage.PackageGUID, "");

            // sorting attributes and methods
            Tool.sortAttributesAndMethods(repository, cppEnergisticsPackage);

            Tool.log(repository, "Energistics model transformed into a C++ model.");
        }

        /// <summary>
        /// Generates the fesapi sources (only headers) from the C++ Energistics model (cf. 
        /// transformEnergisticsModelToCppModel method). This is the last step of the fesapi generation.
        /// This code generation relies on the "C++ Fesapi" code generation template
        /// </summary>
        /// <param name="cppEnergisticsModel"></param>
        /// <param name="outputPath"></param>
        private void generateCodeFromEnergisticsModelAlteration(EA.Package cppEnergisticsModel, string outputPath)
        {
            Tool.log(repository, "Generating fesapi code from Energistics model alteration...");

            // getting the list of classes to generate
            List<EA.Element> toGenerateClassList = new List<EA.Element>();
            Tool.fillElementList(cppEnergisticsModel, toGenerateClassList);

            // setting the code output path for each class
            EA.Project project = repository.GetProjectInterface();
            foreach (EA.Element c in toGenerateClassList)
            {
                EA.TaggedValue fesapiNamespaceTag = c.TaggedValues.GetByName("fesapiNamespace");

                if (fesapiNamespaceTag.Value == "common") // common classes are generated into the root of the output path
                    c.Genfile = outputPath + "\\" + c.Name + ".h";
                else // other classes are generated into a sub-directory named according to the namespace of fesapiNamespace tag value
                    c.Genfile = outputPath + "\\" + fesapiNamespaceTag.Value + "\\" + c.Name + ".h";
                
                if (!(c.Update()))
                {
                    Tool.showMessageBox(repository, c.GetLastError());
                    continue;
                }
            }

            // generating the code
            project.GeneratePackage(cppEnergisticsModel.PackageGUID, "recurse=1;overwrite=1");

            Tool.log(repository, "fesapi code generated.");
        }

        #endregion
    }
}
