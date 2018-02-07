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
using System.Collections.Generic;

namespace fesapiGenerator
{
    /// <summary>
    /// Class responsible for generating the fesapi model
    /// </summary>
    public class FesapiModelGenerator
    {
        #region members

        /// <summary>
        /// EA repository.
        /// </summary>
        private EA.Repository repository;

        /// <summary>
        /// Energistics common model
        /// </summary>
        private EA.Package commonModel;

        /// <summary>
        /// Energistics common/v2.0 package
        /// </summary>
        private EA.Package commonV2Package;

        /// <summary>
        /// Energistics common/v2.2 package
        /// </summary>
        private EA.Package commonV2_2Package;

        /// <summary>
        /// Energistics resqml model
        /// </summary>
        private EA.Package resqmlModel;

        /// <summary>
        /// Energistics resqml/V2.0.1 package
        /// </summary>
        private EA.Package resqmlV2_0_1Package;

        /// <summary>
        /// Energistics resqml/V2.2 package
        /// </summary>
        private EA.Package resqmlV2_2Package;

        /// <summary>
        /// Fesapi model
        /// </summary>
        private EA.Package fesapiModel;

        /// <summary>
        /// Fesapi common package
        /// </summary>
        private EA.Package fesapiCommonPackage;

        /// <summary>
        /// Fesapi resqml2 package
        /// </summary>
        private EA.Package fesapiResqml2Package;

        /// <summary>
        /// Fesapi resqml2_0_1 package
        /// </summary>
        private EA.Package fesapiResqml2_0_1Package;

        /// <summary>
        /// Fesapi resqml2_2 package
        /// </summary>
        private EA.Package fesapiResqml2_2Package;

        private EA.Element epcDocument;
        private EA.Element abstractObject;
        
        private List<EA.Element> fesapiResqml2ClassList = null;
        private List<EA.Element> fesapiResqml2_0_1ClassList = null;
        private List<EA.Element> fesapiResqml2_2ClassList = null;

        private Dictionary<EA.Element, EA.Element> fesapiResqml2ToEnergisticsResqml2_0_1 = null;
        private Dictionary<EA.Element, EA.Element> fesapiResqml2ToEnergisticsResqml2_2 = null;
        private Dictionary<EA.Element, EA.Element> fesapiResqml2_0_1toEnergisticsResqml2_0_1 = null;
        private Dictionary<EA.Element, EA.Element> fesapiResqml2_2toEnergisticsResqml2_2 = null;

        #endregion

        #region constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="repository">EA repository</param>
        /// <param name="commonModel">Energistics common model</param>
        /// <param name="resqmlModel">Energistics resqml model</param>
        /// <param name="fesapiModel">fesapi model</param>
        public FesapiModelGenerator(
            EA.Repository repository, 
            EA.Package commonModel, EA.Package commonV2Package, EA.Package commonV2_2Package,
            EA.Package resqmlModel, EA.Package resqmlV2_0_1Package, EA.Package resqmlV2_2Package,
            EA.Package fesapiModel, EA.Package fesapiCommonPackage, EA.Package fesapiResqml2Package, EA.Package fesapiResqml2_0_1Package, EA.Package fesapiResqml2_2Package)
        {
            this.repository = repository;
            this.commonModel = commonModel;
            this.commonV2Package = commonV2Package;
            this.commonV2_2Package = commonV2_2Package;
            this.resqmlModel = resqmlModel;
            this.resqmlV2_0_1Package = resqmlV2_0_1Package;
            this.resqmlV2_2Package = resqmlV2_2Package;
            this.fesapiModel = fesapiModel;
            this.fesapiCommonPackage = fesapiCommonPackage;
            this.fesapiResqml2Package = fesapiResqml2Package;
            this.fesapiResqml2_0_1Package = fesapiResqml2_0_1Package;
            this.fesapiResqml2_2Package = fesapiResqml2_2Package;

            fesapiResqml2ClassList = new List<EA.Element>();
            fesapiResqml2_0_1ClassList = new List<EA.Element>();
            fesapiResqml2_2ClassList = new List<EA.Element>();

            fesapiResqml2ToEnergisticsResqml2_0_1 = new Dictionary<EA.Element, EA.Element>();
            fesapiResqml2ToEnergisticsResqml2_2 = new Dictionary<EA.Element, EA.Element>();
            fesapiResqml2_0_1toEnergisticsResqml2_0_1 = new Dictionary<EA.Element, EA.Element>();
            fesapiResqml2_2toEnergisticsResqml2_2 = new Dictionary<EA.Element, EA.Element>();
        }

        #endregion

        #region public methods

        /// <summary>
        ///  Generate the fesapi model according to input common and resqml models.
        /// </summary>
        public void generateFesapiModel()
        {
            // Adding an AbstractObject class in the fesapi/common package
            epcDocument = fesapiCommonPackage.Elements.AddNew("EpcDocument", "Class");
            if (!(epcDocument.Update()))
            {
                Tool.showMessageBox(repository, epcDocument.GetLastError());
                return;
            }

            // Adding an AbstractObject class in the fesapi/common package
            abstractObject = fesapiCommonPackage.Elements.AddNew("AbstractObject", "Class");
            if (!(abstractObject.Update()))
            {
                Tool.showMessageBox(repository, abstractObject.GetLastError());
                return;
            }
            
            // handling common classes
            Tool.log(repository, "Looking for top level classes existing in both Resqml v2.0.1 and Resqml v2.2");
            List<EA.Element> energisticsResqml2_0_1ClassList = new List<EA.Element>();
            List<EA.Element> energisticsResqml2_2ClassList = new List<EA.Element>();
            Tool.fillElementList(resqmlV2_0_1Package, energisticsResqml2_0_1ClassList, "Class", "XSDtopLevelElement");
            Tool.fillElementList(resqmlV2_2Package, energisticsResqml2_2ClassList, "Class", "XSDtopLevelElement");
            foreach (EA.Element energisticsResqml2_0_1Class in energisticsResqml2_0_1ClassList)
            {
                string className = energisticsResqml2_0_1Class.Name;

                // for debugging purpose (accelerating process)
                if (!(className.Equals("AbstractLocal3dCrs")) && !(className.Equals("LocalDepth3dCrs")) && !(className.Equals("LocalTime3dCrs")))
                    continue;

                EA.Element energisticsResqml2_2Class = energisticsResqml2_2ClassList.Find(c => c.Name.Equals(className));
                if (energisticsResqml2_2Class != null)
                {
                    Tool.log(repository, className);

                    EA.Element fesapiResqml2Class = fesapiResqml2Package.Elements.AddNew(className, "Class");
                    if (!(fesapiResqml2Class.Update()))
                    {
                        Tool.showMessageBox(repository, fesapiResqml2Class.GetLastError());
                        return;
                    }
                    fesapiResqml2ClassList.Add(fesapiResqml2Class);
                    fesapiResqml2ToEnergisticsResqml2_0_1.Add(fesapiResqml2Class, energisticsResqml2_0_1Class);
                    fesapiResqml2ToEnergisticsResqml2_2.Add(fesapiResqml2Class, energisticsResqml2_2Class);

                    // if it is not an abstract class, it must be added to both resqml2_0_1 and resqml2_2 fesapi packages
                    if (!(className.StartsWith("Abstract")))
                    {
                        EA.Element fesapiResqml2_0_1Class = fesapiResqml2_0_1Package.Elements.AddNew(className, "Class");
                        if (!(fesapiResqml2_0_1Class.Update()))
                        {
                            Tool.showMessageBox(repository, fesapiResqml2_0_1Class.GetLastError());
                            return;
                        }
                        fesapiResqml2_0_1ClassList.Add(fesapiResqml2_0_1Class);
                        fesapiResqml2_0_1toEnergisticsResqml2_0_1.Add(fesapiResqml2_0_1Class, energisticsResqml2_0_1Class);

                        EA.Element fesapiResqml2_2Class = fesapiResqml2_2Package.Elements.AddNew(className, "Class");
                        if (!(fesapiResqml2_2Class.Update()))
                        {
                            Tool.showMessageBox(repository, fesapiResqml2_2Class.GetLastError());
                            return;
                        }
                        fesapiResqml2_2ClassList.Add(fesapiResqml2_2Class);
                        fesapiResqml2_2toEnergisticsResqml2_2.Add(fesapiResqml2_2Class, energisticsResqml2_2Class);
                    }

                    //// collecte des attributs communs.
                    //Tool.log(repository, "Looking for common attributes");
                    //foreach (EA.Attribute resqmlV2_0_1Attribute in resqmlV2_0_1Class.Attributes)
                    //{
                    //    EA.Attribute commonAttribute = null;
                    //    foreach (EA.Attribute a in resqmlV2_2Class.Attributes)
                    //    {
                    //        if (resqmlV2_0_1Attribute.Name == a.Name)
                    //        {
                    //            commonAttribute = a;
                    //            break;
                    //        }
                    //    }

                    //    if (commonAttribute != null)
                    //    {
                    //        EA.Attribute newAttribute = newClass.Attributes.AddNew(commonAttribute.Name, "void");
                    //        if (!(newAttribute.Update()))
                    //        {
                    //            Tool.showMessageBox(repository, newAttribute.GetLastError());
                    //            return;
                    //        }
                    //    }

                    //    newClass.Attributes.Refresh();
                    //    newClass.Refresh();
                    //    Tool.log(repository, resqmlV2_0_1Attribute.Name);
                    //}
                }
            }

            inheritanceSetting();

            fesapiCommonPackage.Elements.Refresh();
            fesapiResqml2Package.Elements.Refresh();
            fesapiResqml2_0_1Package.Elements.Refresh();
            fesapiResqml2_2Package.Elements.Refresh();

            // création des classes communes dans test
            //foreach (string resqmlV2ClassName in resqmlV2ClassNameList)
            //{
            //    EA.Element newClass = testPackage.Elements.AddNew(resqmlV2ClassName, "Class");
            //    if (!(newClass.Update()))
            //    {
            //        Tool.showMessageBox(repository, newClass.GetLastError());
            //        return;
            //    }
            //    newClass.Refresh();
            //}

            // make sure the model view is up to date in the Enterprise Architect GUI
            repository.RefreshModelView(0);
        }

        #endregion

        #region private

        /// <summary>
        /// 
        /// </summary>
        private void inheritanceSetting()
        {
            foreach (EA.Element resqml2Class in fesapiResqml2ClassList)
            {
                if (fesapiResqml2ToEnergisticsResqml2_0_1[resqml2Class].BaseClasses.GetAt(0).Name.Equals("AbstractResqmlDataObject") &&
                    fesapiResqml2ToEnergisticsResqml2_2[resqml2Class].BaseClasses.GetAt(0).Name.Equals("AbstractObject"))
                {
                    (resqml2Class.BaseClasses.AddNew(abstractObject.ElementGUID, "Class")).update();
                    resqml2Class.BaseClasses.Refresh();
                    resqml2Class.Update();
                }
            }


            
        }

        #endregion

    }
}
