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
using System.Xml;

namespace fesapiGenerator
{
    /// <summary>
    /// Class responsible for generating the fesapi model
    /// </summary>
    public class FesapiModelGenerator
    {
        #region members

        /// <summary>
        /// Fesapi generation XML configuration file
        /// </summary>
        private XmlDocument codeConfigurationFile;

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
            XmlDocument xmlConf,
            EA.Repository repository,
            EA.Package commonModel, EA.Package commonV2Package, EA.Package commonV2_2Package,
            EA.Package resqmlModel, EA.Package resqmlV2_0_1Package, EA.Package resqmlV2_2Package,
            EA.Package fesapiModel, EA.Package fesapiCommonPackage, EA.Package fesapiResqml2Package, EA.Package fesapiResqml2_0_1Package, EA.Package fesapiResqml2_2Package)
        {
            this.codeConfigurationFile = xmlConf;
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
            epcDocument = addFesapiClass("EpcDocument", fesapiCommonPackage);
            if (epcDocument == null)
            {
                return;
            }

            // Adding an AbstractObject class in the fesapi/common package
            abstractObject = addFesapiClass("AbstractObject", fesapiCommonPackage);
            if (abstractObject == null)
            {
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
                    EA.Element fesapiResqml2Class = addFesapiClass(className, fesapiResqml2Package);
                    if (fesapiResqml2Class != null)
                    {
                        fesapiResqml2ClassList.Add(fesapiResqml2Class);
                        fesapiResqml2ToEnergisticsResqml2_0_1.Add(fesapiResqml2Class, energisticsResqml2_0_1Class);
                        fesapiResqml2ToEnergisticsResqml2_2.Add(fesapiResqml2Class, energisticsResqml2_2Class);
                    }

                    // if it is not an abstract class, it must be added to both resqml2_0_1 and resqml2_2 fesapi packages
                    if (!(className.StartsWith("Abstract")))
                    {
                        EA.Element fesapiResqml2_0_1Class = addFesapiClass(className, fesapiResqml2_0_1Package);
                        if (fesapiResqml2_0_1Class != null)
                        {
                            fesapiResqml2_0_1ClassList.Add(fesapiResqml2_0_1Class);
                            fesapiResqml2_0_1toEnergisticsResqml2_0_1.Add(fesapiResqml2_0_1Class, energisticsResqml2_0_1Class);
                        }

                        EA.Element fesapiResqml2_2Class = addFesapiClass(className, fesapiResqml2_2Package);
                        if (fesapiResqml2_2Class != null)
                        {
                            fesapiResqml2_2ClassList.Add(fesapiResqml2_2Class);
                            fesapiResqml2_2toEnergisticsResqml2_2.Add(fesapiResqml2_2Class, energisticsResqml2_2Class);
                        }
                    }
                }
                else // energisticsResqml2_0_1Class is not a Resqml 2.0.1/Resqml 2.2 common class
                {
                    EA.Element fesapiResqml2_0_1Class = addFesapiClass(className, fesapiResqml2_0_1Package);
                    if (fesapiResqml2_0_1Class != null)
                    {
                        fesapiResqml2_0_1ClassList.Add(fesapiResqml2_0_1Class);
                        fesapiResqml2_0_1toEnergisticsResqml2_0_1.Add(fesapiResqml2_0_1Class, energisticsResqml2_0_1Class);
                    }
                }
            }

            //// looking for Resqml 2.2 classes which are not common with Resqml 2.0.1
            //foreach (EA.Element energisticsResqml2_2Class in energisticsResqml2_2ClassList)
            //{
            //    string className = energisticsResqml2_2Class.Name;

            //    EA.Element energisticsResqml2_0_1Class = energisticsResqml2_0_1ClassList.Find(c => c.Name.Equals(className));
            //    if (energisticsResqml2_0_1Class == null)
            //    {
            //        EA.Element fesapiResqml2_2Class = addFesapiClass(className, fesapiResqml2_2Package);
            //        if (fesapiResqml2_2Class != null)
            //        {
            //            fesapiResqml2_2ClassList.Add(fesapiResqml2_2Class);
            //            fesapiResqml2_2toEnergisticsResqml2_2.Add(fesapiResqml2_2Class, energisticsResqml2_2Class);
            //        }
            //    }
            //}

            fesapiCommonPackage.Elements.Refresh();
            fesapiResqml2Package.Elements.Refresh();
            fesapiResqml2_0_1Package.Elements.Refresh();
            fesapiResqml2_2Package.Elements.Refresh();

            inheritanceSetting();
            constructorSetting();
            xmlTagSetting();
            getterSetting();

            // make sure the model view is up to date in the Enterprise Architect GUI
            repository.RefreshModelView(0);
        }

        #endregion

        #region private methods

        private EA.Element addFesapiClass(string name, EA.Package fesapiPackage)
        {
            EA.Element fesapiClass = fesapiPackage.Elements.AddNew(name, "Class");
            fesapiClass.Gentype = "C++ Fesapi 2";
            if (!(fesapiClass.Update()))
            {
                Tool.showMessageBox(repository, fesapiClass.GetLastError());
                return null;
            }
            fesapiClass.Elements.Refresh();

            return fesapiClass;
        }

        private EA.Connector addGeneralizationConnector(EA.Element childClass, EA.Element baseClass)
        {
            EA.Connector generalizationConnector = childClass.Connectors.AddNew("", "Generalization");
            generalizationConnector.SupplierID = baseClass.ElementID;
            if (!(generalizationConnector.Update()))
            {
                Tool.showMessageBox(repository, generalizationConnector.GetLastError());
                return null;
            }
            childClass.Connectors.Refresh();

            // we set up the #include directive to be copied in the generated code
            string baseClassPackageName = repository.GetPackageByID(baseClass.PackageID).Name;
            string childclassPackageName = repository.GetPackageByID(childClass.PackageID).Name;
            string includeTagValue = "";
            //if (baseClassPackageName != childclassPackageName) // if the fesapi class and fesapi base class belong to a different
            //{
                // package, we need to provide the relative path of the header file
                includeTagValue += baseClassPackageName + "/";
            //}
            includeTagValue += baseClass.Name + ".h";

            // tagging the child class with the #include directive
            EA.TaggedValue fesapiIncludeTag = childClass.TaggedValues.AddNew(Constants.fesapiBaseClassIncludeTagName, includeTagValue);
            if (!(fesapiIncludeTag.Update()))
            {
                Tool.showMessageBox(repository, fesapiIncludeTag.GetLastError());
            }

            // tagging the child class with the fesapi base class namespace (that is to say the name of its parent package)
            EA.TaggedValue fesapiBaseClassNamespaceTag = childClass.TaggedValues.AddNew("fesapiBaseClassNamespace", baseClassPackageName);
            if (!(fesapiBaseClassNamespaceTag.Update()))
            {
                Tool.showMessageBox(repository, fesapiBaseClassNamespaceTag.GetLastError());
            }

            childClass.TaggedValues.Refresh();

            return generalizationConnector;
        }

        /// <summary>
        /// 
        /// </summary>
        private void inheritanceSetting()
        {
            Tool.log(repository, "enheritanceSetting()...");

            Tool.log(repository, "resqml2 classes");

            // handling fesapi/resqml2 classes
            foreach (EA.Element resqml2Class in fesapiResqml2ClassList)
            {
                EA.Element energisticsResqml2_0_1Class = fesapiResqml2ToEnergisticsResqml2_0_1[resqml2Class];
                EA.Element energisticsResqml2_2Class = fesapiResqml2ToEnergisticsResqml2_2[resqml2Class];

                if ((energisticsResqml2_0_1Class.BaseClasses.Count == 0) || (energisticsResqml2_2Class.BaseClasses.Count == 0))
                {
                    continue; // should never happened
                }

                // first, we look for fesapi/resqml2 classes inheriting from fesapi/common/AbstracObject
                if (energisticsResqml2_0_1Class.BaseClasses.GetAt(0).Name.Equals("AbstractResqmlDataObject") &&
                    energisticsResqml2_2Class.BaseClasses.GetAt(0).Name.Equals("AbstractObject"))
                {
                    addGeneralizationConnector(resqml2Class, abstractObject);
                }
                else // then, we look for fesapi/resqml2 classes inheriting from fesapi/resqml2 classes
                {
                    if (!(energisticsResqml2_0_1Class.BaseClasses.GetAt(0).Name.Equals(energisticsResqml2_2Class.BaseClasses.GetAt(0).Name)))
                    {
                        Tool.log(repository, resqml2Class.Name + " exists in both Resqml 2.0.1 and Resqml 2.2 but inherits from different classes.");
                        Tool.log(repository, "No generalization link will be generated !");
                        continue;
                    }

                    EA.Element resqml2ParentClass = fesapiResqml2ClassList.Find(c => energisticsResqml2_2Class.BaseClasses.GetAt(0).Name.Equals(c.Name));
                    if (resqml2ParentClass != null)
                    {
                        addGeneralizationConnector(resqml2Class, resqml2ParentClass);
                    }
                }
            }

            Tool.log(repository, "resqml2_0_1 classes");

            // handling fesapi/resqml2_0_1 classes
            foreach (EA.Element resqml2_0_1Class in fesapiResqml2_0_1ClassList)
            {
                EA.Element energisticsResqml2_0_1Class = fesapiResqml2_0_1toEnergisticsResqml2_0_1[resqml2_0_1Class];

                if (energisticsResqml2_0_1Class.BaseClasses.Count == 0)
                    continue; // should never happened

                // first, we look for fesapi/resqml2_0_1 classes inheriting from  fesapi/common/AbstractObject
                if (energisticsResqml2_0_1Class.BaseClasses.GetAt(0).Name.Equals("AbstractResqmlDataObject"))
                {
                    addGeneralizationConnector(resqml2_0_1Class, abstractObject);
                 }
                else
                {
                    // then, we look if the parent class is in fesapi/resqml2_0_1 
                    EA.Element resqml2_0_1ParentClass = fesapiResqml2_0_1ClassList.Find(c => energisticsResqml2_0_1Class.BaseClasses.GetAt(0).Name.Equals(c.Name));
                    if (resqml2_0_1ParentClass != null)
                    {
                        addGeneralizationConnector(resqml2_0_1Class, resqml2_0_1ParentClass);
                    }
                    else
                    {
                        // then, we look if there is a class in fesapi/resqml2 with the same name
                        EA.Element resqml2ParentClass = fesapiResqml2ClassList.Find(c => c.Name.Equals(resqml2_0_1Class.Name));
                        if (resqml2ParentClass == null)
                        {
                            // finally, we look if the parent class is in fesapi/resqml2
                            resqml2ParentClass = fesapiResqml2ClassList.Find(c => energisticsResqml2_0_1Class.BaseClasses.GetAt(0).Name.Equals(c.Name));
                        }

                        if (resqml2ParentClass != null) // should always happened
                        {
                            addGeneralizationConnector(resqml2_0_1Class, resqml2ParentClass);
                        }
                    }

                }
            }

            Tool.log(repository, "resqml2_2 classes");

            // handling fesapi/resqml2_2 classes
            foreach (EA.Element resqml2_2Class in fesapiResqml2_2ClassList)
            {
                EA.Element energisticsResqml2_2Class = fesapiResqml2_2toEnergisticsResqml2_2[resqml2_2Class];

                if (energisticsResqml2_2Class.BaseClasses.Count == 0)
                    continue; // should never happened

                // first, we look for fesapi/resqml2_2 classes inheriting from  fesapi/common/AbstractObject
                if (energisticsResqml2_2Class.BaseClasses.GetAt(0).Name.Equals("AbstractResqmlDataObject"))
                {
                    addGeneralizationConnector(resqml2_2Class, abstractObject);
                }
                else
                {
                    // then, we look if the parent class is in fesapi/resqml2_2 
                    EA.Element resqml2_2ParentClass = fesapiResqml2_2ClassList.Find(c => energisticsResqml2_2Class.BaseClasses.GetAt(0).Name.Equals(c.Name));
                    if (resqml2_2ParentClass != null)
                    {
                        addGeneralizationConnector(resqml2_2Class, resqml2_2ParentClass);
                    }
                    else
                    {
                        // then, we look if there is a class in fesapi/resqml2 with the same name
                        EA.Element resqml2ParentClass = fesapiResqml2ClassList.Find(c => c.Name.Equals(resqml2_2Class.Name));
                        if (resqml2ParentClass == null)
                        {
                            // finally, we look if the parent class is in fesapi/resqml2
                            resqml2ParentClass = fesapiResqml2ClassList.Find(c => energisticsResqml2_2Class.BaseClasses.GetAt(0).Name.Equals(c.Name));
                        }

                        if (resqml2ParentClass != null) // should always happened
                        {
                            addGeneralizationConnector(resqml2_2Class, resqml2ParentClass);
                        }
                    }

                }
            }

            Tool.log(repository, "inheritanceSetting() end.");
        }

        private EA.Method addDefaultConstructor(EA.Element fesapiClass, string visibility)
        {
            EA.Method constructor = fesapiClass.Methods.AddNew(fesapiClass.Name, "");
            constructor.Notes = "Default constructor\nSet the gsoap proxy to nullptr.";
            constructor.Visibility =visibility;
            if (!(constructor.Update()))
            {
                Tool.showMessageBox(repository, constructor.GetLastError());
                return null;
            }
            fesapiClass.Methods.Refresh();

            // adding a tag sepcifying that the body will be located into the class declaration
            EA.MethodTag bodyLocationTag = constructor.TaggedValues.AddNew("bodyLocation", "classDec");
            if (!(bodyLocationTag.Update()))
            {
                Tool.showMessageBox(repository, bodyLocationTag.GetLastError());
                return null;
            }
            constructor.TaggedValues.Refresh();

            return constructor;
        }

        private EA.Method addDefaultDestructor(EA.Element fesapiClass)
        {
            EA.Method destructor = fesapiClass.Methods.AddNew("~" + fesapiClass.Name, "");
            destructor.Notes = "Destructor does nothing since the memory is managed by the gsoap context.";
            destructor.Abstract = true; // specify that the destructor is virtual
            destructor.Visibility = "public"; // destructor always public
            if (!(destructor.Update()))
            {
                Tool.showMessageBox(repository, destructor.GetLastError());
                return null;
            }
            fesapiClass.Methods.Refresh();

            // adding a tag sepcifying that the body will be located into the class declaration
            EA.MethodTag bodyLocationTag = destructor.TaggedValues.AddNew("bodyLocation", "classDec");
            if (!(bodyLocationTag.Update()))
            {
                Tool.showMessageBox(repository, bodyLocationTag.GetLastError());
                return null;
            }
            destructor.TaggedValues.Refresh();
            
            return destructor;
        }

        private EA.Method addPartialTransfertConstructor(EA.Element fesapiClass, string visibility, string gsoapPrefix)
        {
            EA.Method constructor = fesapiClass.Methods.AddNew(fesapiClass.Name, "");
            constructor.Code = "";
            constructor.Notes = "Only to be used in partial transfer context";
            constructor.Visibility = visibility;
            if (!(constructor.Update()))
            {
                Tool.showMessageBox(repository, constructor.GetLastError());
                return null;
            }
            fesapiClass.Methods.Refresh();

            // adding a tag sepcifying that the body will be located into the class declaration
            EA.MethodTag bodyLocationTag = constructor.TaggedValues.AddNew("bodyLocation", "classDec");
            if (!(bodyLocationTag.Update()))
            {
                Tool.showMessageBox(repository, bodyLocationTag.GetLastError());
                return null;
            }
            constructor.TaggedValues.Refresh();

            // adding the partial transfert parameter
            EA.Parameter parameter = constructor.Parameters.AddNew("partialObject", gsoapPrefix + "DataObjectReference* ");
            if (!(parameter.Update()))
            {
                Tool.showMessageBox(repository, parameter.GetLastError());
                return null;
            }
            constructor.Parameters.Refresh();

            // adding the initialization part
            if (fesapiClass.BaseClasses.Count != 0)
            {
                EA.Element baseClass = fesapiClass.BaseClasses.GetAt(0);
                string baseClassPackageName = repository.GetPackageByID(baseClass.PackageID).Name;
                string childclassPackageName = repository.GetPackageByID(fesapiClass.PackageID).Name;

                string initializerTagValue = "";
                if (!(baseClassPackageName.Equals(childclassPackageName)))
                {
                    initializerTagValue += baseClassPackageName + "::";
                }
                initializerTagValue += baseClass.Name + "(partialObject)";
                EA.MethodTag initializerTag = constructor.TaggedValues.AddNew("initializer", initializerTagValue);
                if (!(initializerTag.Update()))
                {
                    Tool.showMessageBox(repository, initializerTag.GetLastError());
                }
                constructor.TaggedValues.Refresh();
            }

            return constructor;
        }

        private EA.Method addDeserializationConstructor(EA.Element fesapiClass, string visibility, string gsoapPrefix)
        {
            EA.Method constructor = fesapiClass.Methods.AddNew(fesapiClass.Name, "");
            constructor.Code = "";
            constructor.Notes = "Creates an instance of this class by wrapping a gsoap instance.";
            constructor.Visibility = visibility;
            if (!(constructor.Update()))
            {
                Tool.showMessageBox(repository, constructor.GetLastError());
            }
            fesapiClass.Methods.Refresh();

            // adding a tag sepcifying that the body will be located into the class declaration
            EA.MethodTag bodyLocationTag = constructor.TaggedValues.AddNew("bodyLocation", "classDec");
            if (!(bodyLocationTag.Update()))
            {
                Tool.showMessageBox(repository, bodyLocationTag.GetLastError());
            }

            // adding the deserialization parameter
            EA.Parameter parameter = constructor.Parameters.AddNew("fromGsoap", gsoapPrefix + fesapiClass.Name + "* ");
            if (!(parameter.Update()))
            {
                Tool.showMessageBox(repository, parameter.GetLastError());
            }
            constructor.Parameters.Refresh();

            // adding the initialization part
            if (fesapiClass.BaseClasses.Count != 0)
            {
                EA.Element baseClass = fesapiClass.BaseClasses.GetAt(0);
                string baseClassPackageName = repository.GetPackageByID(baseClass.PackageID).Name;
                string childclassPackageName = repository.GetPackageByID(fesapiClass.PackageID).Name;

                string initializerTagValue = "";
                if (!(baseClassPackageName.Equals(childclassPackageName)))
                {
                    initializerTagValue += baseClassPackageName + "::";
                }
                initializerTagValue += baseClass.Name + "(fromGsoap)";
                EA.MethodTag initializerTag = constructor.TaggedValues.AddNew("initializer", initializerTagValue);
                if (!(initializerTag.Update()))
                {
                    Tool.showMessageBox(repository, initializerTag.GetLastError());
                }
            }

            return constructor;
        }

        private void constructorSetting()
        {
            string xmlEqml2_0GsoapPrefix = codeConfigurationFile.DocumentElement.SelectSingleNode("/FesapiGenConf/Constants/Eml2_0GsoapPrefix").Attributes["value"].Value;
            string xmlEqml2_2GsoapPrefix = codeConfigurationFile.DocumentElement.SelectSingleNode("/FesapiGenConf/Constants/Eml2_2GsoapPrefix").Attributes["value"].Value;
            string xmlResqml2_0_1GsoapPrefix = codeConfigurationFile.DocumentElement.SelectSingleNode("/FesapiGenConf/Constants/Resqml2_0_1GsoapPrefix").Attributes["value"].Value;
            string xmlResqml2_0_1AbstractGsoapPrefix = codeConfigurationFile.DocumentElement.SelectSingleNode("/FesapiGenConf/Constants/Resqml2_0_1AbstractGsoapPrefix").Attributes["value"].Value;
            string xmlResqml2_2GsoapPrefix = codeConfigurationFile.DocumentElement.SelectSingleNode("/FesapiGenConf/Constants/Resqml2_2GsoapPrefix").Attributes["value"].Value;
            string xmlResqml2_2AbstractGsoapPrefix = codeConfigurationFile.DocumentElement.SelectSingleNode("/FesapiGenConf/Constants/Resqml2_2AbstractGsoapPrefix").Attributes["value"].Value;


            // handling fesapi/resqml2 classes
            foreach (EA.Element resqml2Class in fesapiResqml2ClassList)
            {
                addDefaultConstructor(resqml2Class, "protected");
                addDefaultDestructor(resqml2Class);
                addPartialTransfertConstructor(resqml2Class, "protected", xmlEqml2_0GsoapPrefix);
                addPartialTransfertConstructor(resqml2Class, "protected", xmlEqml2_2GsoapPrefix);
                if (Tool.isAbstract(resqml2Class))
                {
                    addDeserializationConstructor(resqml2Class, "protected", xmlResqml2_0_1AbstractGsoapPrefix);
                    addDeserializationConstructor(resqml2Class, "protected", xmlResqml2_2AbstractGsoapPrefix);
                }
                else
                {
                    addDeserializationConstructor(resqml2Class, "protected", xmlResqml2_0_1GsoapPrefix);
                    addDeserializationConstructor(resqml2Class, "protected", xmlResqml2_2GsoapPrefix);
                }
            }

            // handling fesapi/resqml2_0_1 classes
            foreach (EA.Element resqml2_0_1Class in fesapiResqml2_0_1ClassList)
            {
                string constructorVisibility;
                if (Tool.isAbstract(resqml2_0_1Class))
                {
                    constructorVisibility = "protected";
                }
                else
                {
                    constructorVisibility = "public";
                }

                addDefaultConstructor(resqml2_0_1Class, constructorVisibility);
                addDefaultDestructor(resqml2_0_1Class);
                addPartialTransfertConstructor(resqml2_0_1Class, constructorVisibility, xmlEqml2_0GsoapPrefix);
                if (Tool.isAbstract(resqml2_0_1Class))
                {
                    addDeserializationConstructor(resqml2_0_1Class, constructorVisibility, xmlResqml2_0_1AbstractGsoapPrefix);
                }
                else
                {
                    addDeserializationConstructor(resqml2_0_1Class, constructorVisibility, xmlResqml2_0_1GsoapPrefix);
                }
            }

            // handling fesapi/resqml2_2 classes
            foreach (EA.Element resqml2_2Class in fesapiResqml2_2ClassList)
            {
                string constructorVisibility;
                if (Tool.isAbstract(resqml2_2Class))
                {
                    constructorVisibility = "protected";
                }
                else
                {
                    constructorVisibility = "public";
                }

                addDefaultConstructor(resqml2_2Class, constructorVisibility);
                addDefaultDestructor(resqml2_2Class);
                addPartialTransfertConstructor(resqml2_2Class, constructorVisibility, xmlEqml2_2GsoapPrefix);
                if (Tool.isAbstract(resqml2_2Class))
                {
                    addDeserializationConstructor(resqml2_2Class, constructorVisibility, xmlResqml2_2AbstractGsoapPrefix);
                }
                else
                {
                    addDeserializationConstructor(resqml2_2Class, constructorVisibility, xmlResqml2_2GsoapPrefix);
                }
            }
        }

        /// <summary>
        /// Add an XML_TAG attribute with its getter to a given fesapi class. 
        /// </summary>
        /// <param name="energisticsClass">A fesapi class</param>
        private void addXmlTagAttributeWithGetter(EA.Element fesapiClass)
        {
            // we add an XML_TAG attribute
            EA.Attribute xmlTagAttribute = fesapiClass.Attributes.AddNew("XML_TAG", "char*");
            xmlTagAttribute.IsStatic = true;
            xmlTagAttribute.IsConst = true;
            xmlTagAttribute.Default = "\"" + fesapiClass.Name + "\"";
            if (!(xmlTagAttribute.Update()))
            {
                Tool.showMessageBox(repository, xmlTagAttribute.GetLastError());
            }
            fesapiClass.Attributes.Refresh();

            // add the XML_TAG attributr getter
            EA.Method getXmlTagMethod = fesapiClass.Methods.AddNew("getXmlTag", "std::string");
            getXmlTagMethod.Stereotype = "const";
            getXmlTagMethod.Code = "return XML_TAG;";
            getXmlTagMethod.Abstract = true;
            if (!(getXmlTagMethod.Update()))
            {
                Tool.showMessageBox(repository, getXmlTagMethod.GetLastError());
            }
            fesapiClass.Methods.Refresh();

            // tag the getter in order for its body to be generated into the class declaration
            EA.MethodTag getXmlTagMethodBodyLocationTag = getXmlTagMethod.TaggedValues.AddNew("bodyLocation", "classDec");
            if (!(getXmlTagMethodBodyLocationTag.Update()))
            {
                Tool.showMessageBox(repository, getXmlTagMethodBodyLocationTag.GetLastError());
            }
            getXmlTagMethod.TaggedValues.Refresh();
        }

        private void xmlTagSetting()
        {
            // handling fesapi/resqml2 classes
            foreach (EA.Element resqml2Class in fesapiResqml2ClassList)
            {
                if (!(resqml2Class.Name.StartsWith("Abstract")))
                {
                    addXmlTagAttributeWithGetter(resqml2Class);
                }
            }

            // handling fesapi/resqml2_0_1 classes
            foreach (EA.Element resqml2_0_1Class in fesapiResqml2_0_1ClassList)
            {
                if ((fesapiResqml2ClassList.Find(c => c.Name.Equals(resqml2_0_1Class.Name))) == null)
                {
                    addXmlTagAttributeWithGetter(resqml2_0_1Class);
                }
            }

            // handling fesapi/resqml2_2 classes
            foreach (EA.Element resqml2_2Class in fesapiResqml2_2ClassList)
            {
                if ((fesapiResqml2ClassList.Find(c => c.Name.Equals(resqml2_2Class.Name))) == null)
                {
                    addXmlTagAttributeWithGetter(resqml2_2Class);
                }
            }
        }

        private EA.Method addResqml2SimpleGetter(EA.Element fesapiClass, string attributeName, string attributeType)
        {
            string xmlResqml2_0_1GsoapProxy = codeConfigurationFile.DocumentElement.SelectSingleNode("/FesapiGenConf/Constants/Resqml2_0_1GsoapProxy").Attributes["value"].Value;
            string xmlResqml2_2GsoapProxy = codeConfigurationFile.DocumentElement.SelectSingleNode("/FesapiGenConf/Constants/Resqml2_2GsoapProxy").Attributes["value"].Value;
            string xmlAttributeAccess = codeConfigurationFile.DocumentElement.SelectSingleNode("/FesapiGenConf/Expressions/AttributeAccess").Attributes["value"].Value;

            string xmlResqml2_0_1GSoapPrefix;
            string xmlResqml2_2GSoapPrefix;
            if (Tool.isAbstract(fesapiClass))
            {
                xmlResqml2_0_1GSoapPrefix = codeConfigurationFile.DocumentElement.SelectSingleNode("/FesapiGenConf/Constants/Resqml2_0_1AbstractGsoapPrefix").Attributes["value"].Value;
                xmlResqml2_2GSoapPrefix = codeConfigurationFile.DocumentElement.SelectSingleNode("/FesapiGenConf/Constants/Resqml2_2AbstractGsoapPrefix").Attributes["value"].Value;
            }
            else
            {
                xmlResqml2_0_1GSoapPrefix = codeConfigurationFile.DocumentElement.SelectSingleNode("/FesapiGenConf/Constants/Resqml2_0_1GsoapPrefix").Attributes["value"].Value;
                xmlResqml2_2GSoapPrefix = codeConfigurationFile.DocumentElement.SelectSingleNode("/FesapiGenConf/Constants/Resqml2_2GsoapPrefix").Attributes["value"].Value;
            }

            string cppAttributeType = Tool.umlToCppType(attributeType);
            EA.Method getter = fesapiClass.Methods.AddNew("get" + attributeName, cppAttributeType);
            getter.Code = "if (" + xmlResqml2_0_1GsoapProxy + " != nullptr)\n";
            getter.Code += "{\n";
            getter.Code += "\treturn " + xmlAttributeAccess.Replace("#PREFIX", xmlResqml2_0_1GSoapPrefix).Replace("#CLASS", fesapiClass.Name).Replace("#PROXY", xmlResqml2_0_1GsoapProxy).Replace("#ATTRIBUTE", attributeName) + ";\n";
            getter.Code += "}\n";
            getter.Code += "else if (" + xmlResqml2_2GsoapProxy + " != nullptr)\n";
            getter.Code += "{\n";
            getter.Code += "\treturn " + xmlAttributeAccess.Replace("#PREFIX", xmlResqml2_2GSoapPrefix).Replace("#CLASS", fesapiClass.Name).Replace("#PROXY", xmlResqml2_2GsoapProxy).Replace("#ATTRIBUTE", attributeName) + ";\n";
            getter.Code += "}\n";
            getter.Code += "else\n";
            getter.Code += "{\n";
            getter.Code += "\tthrow logic_error(\"Not implemented yet\");\n";
            getter.Code += "}";
            getter.Stereotype = "const";
            if (!(getter.Update()))
            {
                Tool.showMessageBox(repository, getter.GetLastError());
            }
            fesapiClass.Methods.Refresh();

            EA.MethodTag bodyLocationTag = getter.TaggedValues.AddNew("bodyLocation", "classBody");
            if (!(bodyLocationTag.Update()))
            {
                Tool.showMessageBox(repository, bodyLocationTag.GetLastError());
            }
            getter.TaggedValues.Refresh();

            return getter;
        }

        private EA.Method addResqml2_0_1SimpleGetter(EA.Element fesapiClass, string attributeName, string attributeType)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("C:\\Users\\Mathieu Poudret\\Documents\\Projets 2017\\fesapi generator\\fesapiGenConf.xml");
            string xmlResqml2_0_1GsoapProxy = xmlDoc.DocumentElement.SelectSingleNode("/FesapiGenConf/Constants/Resqml2_0_1GsoapProxy").Attributes["value"].Value;
            string xmlResqml2_0_1GSoapPrefix;
            if (Tool.isAbstract(fesapiClass))
            {
                xmlResqml2_0_1GSoapPrefix = xmlDoc.DocumentElement.SelectSingleNode("/FesapiGenConf/Constants/Resqml2_0_1AbstractGsoapPrefix").Attributes["value"].Value;
            }
            else
            {
                xmlResqml2_0_1GSoapPrefix = xmlDoc.DocumentElement.SelectSingleNode("/FesapiGenConf/Constants/Resqml2_0_1GsoapPrefix").Attributes["value"].Value;
            }

            string xmlAttributeAccess = xmlDoc.DocumentElement.SelectSingleNode("/FesapiGenConf/Expressions/AttributeAccess").Attributes["value"].Value;

            string cppAttributeType = Tool.umlToCppType(attributeType);
            EA.Method getter = fesapiClass.Methods.AddNew("get" + attributeName, cppAttributeType);
            getter.Code += "\return " + xmlAttributeAccess.Replace("#PREFIX", xmlResqml2_0_1GSoapPrefix).Replace("#CLASS", fesapiClass.Name).Replace("#PROXY", xmlResqml2_0_1GsoapProxy).Replace("#ATTRIBUTE", attributeName) + ";\n";
            getter.Stereotype = "const";
            if (!(getter.Update()))
            {
                Tool.showMessageBox(repository, getter.GetLastError());
            }
            fesapiClass.Methods.Refresh();

            EA.MethodTag bodyLocationTag = getter.TaggedValues.AddNew("bodyLocation", "classBody");
            if (!(bodyLocationTag.Update()))
            {
                Tool.showMessageBox(repository, bodyLocationTag.GetLastError());
            }
            getter.TaggedValues.Refresh();

            return getter;
        }

        private EA.Method addResqml2_2SimpleGetter(EA.Element fesapiClass, string attributeName, string attributeType)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("C:\\Users\\Mathieu Poudret\\Documents\\Projets 2017\\fesapi generator\\fesapiGenConf.xml");
            string xmlResqml2_2GsoapProxy = xmlDoc.DocumentElement.SelectSingleNode("/FesapiGenConf/Constants/Resqml2_2GsoapProxy").Attributes["value"].Value;
            string xmlResqml2_2GSoapPrefix;
            if (Tool.isAbstract(fesapiClass))
            {
                xmlResqml2_2GSoapPrefix = xmlDoc.DocumentElement.SelectSingleNode("/FesapiGenConf/Constants/Resqml2_2AbstractGsoapPrefix").Attributes["value"].Value;
            }
            else
            {
                xmlResqml2_2GSoapPrefix = xmlDoc.DocumentElement.SelectSingleNode("/FesapiGenConf/Constants/Resqml2_2GsoapPrefix").Attributes["value"].Value;
            }

            string xmlAttributeAccess = xmlDoc.DocumentElement.SelectSingleNode("/FesapiGenConf/Expressions/AttributeAccess").Attributes["value"].Value;

            string cppAttributeType = Tool.umlToCppType(attributeType);
            EA.Method getter = fesapiClass.Methods.AddNew("get" + attributeName, cppAttributeType);
            getter.Code += "return " + xmlAttributeAccess.Replace("#PREFIX", xmlResqml2_2GSoapPrefix).Replace("#CLASS", fesapiClass.Name).Replace("#PROXY", xmlResqml2_2GsoapProxy).Replace("#ATTRIBUTE", attributeName) + ";\n";
            getter.Stereotype = "const";
            if (!(getter.Update()))
            {
                Tool.showMessageBox(repository, getter.GetLastError());
            }
            fesapiClass.Methods.Refresh();

            EA.MethodTag bodyLocationTag = getter.TaggedValues.AddNew("bodyLocation", "classBody");
            if (!(bodyLocationTag.Update()))
            {
                Tool.showMessageBox(repository, bodyLocationTag.GetLastError());
            }
            getter.TaggedValues.Refresh();

            return getter;
        }
        
        private EA.Method addResqml2MeasureGetter(EA.Element fesapiClass, string attributeName)
        {
            string xmlResqml2_0_1GsoapProxy = codeConfigurationFile.DocumentElement.SelectSingleNode("/FesapiGenConf/Constants/Resqml2_0_1GsoapProxy").Attributes["value"].Value;
            string xmlResqml2_2GsoapProxy = codeConfigurationFile.DocumentElement.SelectSingleNode("/FesapiGenConf/Constants/Resqml2_2GsoapProxy").Attributes["value"].Value;
            string xmlAttributeAccess = codeConfigurationFile.DocumentElement.SelectSingleNode("/FesapiGenConf/Expressions/AttributeAccess").Attributes["value"].Value;

            string xmlResqml2_0_1GSoapPrefix;
            string xmlResqml2_2GSoapPrefix;
            if (Tool.isAbstract(fesapiClass))
            {
                xmlResqml2_0_1GSoapPrefix = codeConfigurationFile.DocumentElement.SelectSingleNode("/FesapiGenConf/Constants/Resqml2_0_1AbstractGsoapPrefix").Attributes["value"].Value;
                xmlResqml2_2GSoapPrefix = codeConfigurationFile.DocumentElement.SelectSingleNode("/FesapiGenConf/Constants/Resqml2_2AbstractGsoapPrefix").Attributes["value"].Value;
            }
            else
            {
                xmlResqml2_0_1GSoapPrefix = codeConfigurationFile.DocumentElement.SelectSingleNode("/FesapiGenConf/Constants/Resqml2_0_1GsoapPrefix").Attributes["value"].Value;
                xmlResqml2_2GSoapPrefix = codeConfigurationFile.DocumentElement.SelectSingleNode("/FesapiGenConf/Constants/Resqml2_2GsoapPrefix").Attributes["value"].Value;
            }

            EA.Method getter = fesapiClass.Methods.AddNew("get" + attributeName, "double");

            getter.Code = "if (" + xmlResqml2_0_1GsoapProxy + " != nullptr)\n";
            getter.Code += "{\n";
            getter.Code += "\treturn " + xmlAttributeAccess.Replace("#PREFIX", xmlResqml2_0_1GSoapPrefix).Replace("#CLASS", fesapiClass.Name).Replace("#PROXY", xmlResqml2_0_1GsoapProxy).Replace("#ATTRIBUTE", attributeName) + "->__item;\n";
            getter.Code += "}\n";
            getter.Code += "else if (" + xmlResqml2_2GsoapProxy + " != nullptr)\n";
            getter.Code += "{\n";
            getter.Code += "\treturn " + xmlAttributeAccess.Replace("#PREFIX", xmlResqml2_2GSoapPrefix).Replace("#CLASS", fesapiClass.Name).Replace("#PROXY", xmlResqml2_2GsoapProxy).Replace("#ATTRIBUTE", attributeName) + "->__item;\n";
            getter.Code += "}\n";
            getter.Code += "else\n";
            getter.Code += "{\n";
            getter.Code += "\tthrow logic_error(\"Not implemented yet\");\n";
            getter.Code += "}";
            getter.Stereotype = "const";

            if (!(getter.Update()))
            {
                Tool.showMessageBox(repository, getter.GetLastError());
            }
            fesapiClass.Methods.Refresh();

            EA.MethodTag bodyLocationTag = getter.TaggedValues.AddNew("bodyLocation", "classBody");
            if (!(bodyLocationTag.Update()))
            {
                Tool.showMessageBox(repository, bodyLocationTag.GetLastError());
            }
            getter.TaggedValues.Refresh();

            return getter;

        }

        private EA.Method addResqml2EnumConversionGetter(EA.Element fesapiClass, string getterName, string attributeAccess, string resqml2_2EnumName, string resqml2_0_1EnumName)
        {
            string xmlEml2_2GsoapPrefix = codeConfigurationFile.DocumentElement.SelectSingleNode("/FesapiGenConf/Constants/Eml2_2GsoapPrefix").Attributes["value"].Value;
            string xmlCode = codeConfigurationFile.DocumentElement.SelectSingleNode("/FesapiGenConf/Expressions/Resqml2_0_1ToResqml2_2EnumConversionGetter").Attributes["value"].Value;

            EA.Method getter = fesapiClass.Methods.AddNew(getterName, xmlEml2_2GsoapPrefix + resqml2_2EnumName);

            string gSoapClassName;
            if (Tool.isAbstract(fesapiClass))
            {
                gSoapClassName = "resqml2__" + fesapiClass.Name;
            }
            else
            {
                gSoapClassName = "_resqml2__" + fesapiClass.Name;
            }

            getter.Code = xmlCode.Replace("#CLASS", gSoapClassName).Replace("#ATTRIBUTE", attributeAccess).Replace("#R2_2ENUM", resqml2_2EnumName).Replace("#R2_0_1ENUM", resqml2_0_1EnumName);
            getter.Stereotype = "const";

            if (!(getter.Update()))
            {
                Tool.showMessageBox(repository, getter.GetLastError());
            }
            fesapiClass.Methods.Refresh();

            EA.MethodTag bodyLocationTag = getter.TaggedValues.AddNew("bodyLocation", "classBody");
            if (!(bodyLocationTag.Update()))
            {
                Tool.showMessageBox(repository, bodyLocationTag.GetLastError());
            }
            getter.TaggedValues.Refresh();

            return getter;
        }

        private EA.Method addResqml2EnumExtConversionGetter(EA.Element fesapiClass, string getterName, string attributeAccess, string resqml2_2EnumName, string resqml2_0_1EnumName)
        {
            string xmlEml2_2GsoapPrefix = codeConfigurationFile.DocumentElement.SelectSingleNode("/FesapiGenConf/Constants/Eml2_2GsoapPrefix").Attributes["value"].Value;
            string xmlCode = codeConfigurationFile.DocumentElement.SelectSingleNode("/FesapiGenConf/Expressions/Resqml2_0_1ToResqml2_2EnumExtConversionGetter").Attributes["value"].Value;

            EA.Method getter = fesapiClass.Methods.AddNew(getterName, xmlEml2_2GsoapPrefix + resqml2_2EnumName);

            string gSoapClassName;
            if (Tool.isAbstract(fesapiClass))
            {
                gSoapClassName = "resqml2__" + fesapiClass.Name;
            }
            else
            {
                gSoapClassName = "_resqml2__" + fesapiClass.Name;
            }

            getter.Code = xmlCode.Replace("#CLASS", gSoapClassName).Replace("#ATTRIBUTE", attributeAccess).Replace("#R2_2ENUM", resqml2_2EnumName).Replace("#R2_0_1ENUM", resqml2_0_1EnumName);
            getter.Stereotype = "const";

            if (!(getter.Update()))
            {
                Tool.showMessageBox(repository, getter.GetLastError());
            }
            fesapiClass.Methods.Refresh();

            EA.MethodTag bodyLocationTag = getter.TaggedValues.AddNew("bodyLocation", "classBody");
            if (!(bodyLocationTag.Update()))
            {
                Tool.showMessageBox(repository, bodyLocationTag.GetLastError());
            }
            getter.TaggedValues.Refresh();

            return getter;
        }

        private EA.Method addResqml2EnumGetterAsString(EA.Element fesapiClass, string getterName, string enumName)
        {
            string xmlCode = codeConfigurationFile.DocumentElement.SelectSingleNode("/FesapiGenConf/Expressions/Resqml2GetterAsString").Attributes["value"].Value;

            EA.Method getter = fesapiClass.Methods.AddNew(getterName + "AsString", "std::string");

            getter.Code = xmlCode.Replace("#GETTER", getterName).Replace("#ENUM", enumName);
            getter.Stereotype = "const";

            if (!(getter.Update()))
            {
                Tool.showMessageBox(repository, getter.GetLastError());
            }
            fesapiClass.Methods.Refresh();

            EA.MethodTag bodyLocationTag = getter.TaggedValues.AddNew("bodyLocation", "classBody");
            if (!(bodyLocationTag.Update()))
            {
                Tool.showMessageBox(repository, bodyLocationTag.GetLastError());
            }
            getter.TaggedValues.Refresh();

            return getter;
        }

        private void addResqml2Getter(EA.Element fesapiResqml2Class, EA.Attribute energisticsResqml2_0_1Attribute, EA.Attribute energisticsResqml2_2Attribute)
        {
            Tool.log(repository, "common attribute : " + energisticsResqml2_2Attribute.Name);
            Tool.log(repository, "Resqml 2.2 type : " + energisticsResqml2_2Attribute.Type);
            Tool.log(repository, "Resqml 2.0.1 type : " + energisticsResqml2_0_1Attribute.Type);

            // si le type est le même et est basique
            if ((energisticsResqml2_0_1Attribute.Type.Equals(energisticsResqml2_2Attribute.Type)) && (Tool.isBasicType(energisticsResqml2_2Attribute.Type)))
            {
                // on ajoute un getter simple
                addResqml2SimpleGetter(fesapiResqml2Class, energisticsResqml2_0_1Attribute.Name, energisticsResqml2_0_1Attribute.Type);

                Tool.log(repository, "Simple getter generated");
            }
            // sinon si seul l'un des deux types est basiques
            else if ((Tool.isBasicType(energisticsResqml2_0_1Attribute.Type)) || (Tool.isBasicType(energisticsResqml2_2Attribute.Type)))
            {
                // je ne sais pas traiter pour le moment
                Tool.log(repository, "Not able to generate getter");
            } 
            // sinon (les deux types ne sont pas nécessairement les mêmes et aucun des deux n'est basique)
            else 
            {
                // je vais chercher les classifier
                EA.Element baseType2_2 = repository.GetElementByID(energisticsResqml2_2Attribute.ClassifierID);
                if (baseType2_2 != null)
                {
                    Tool.log(repository, "Resqml 2.2 base type : " + baseType2_2.Name);
                }

                EA.Element baseType2_0_1 = repository.GetElementByID(energisticsResqml2_0_1Attribute.ClassifierID);
                if (baseType2_0_1 != null)
                {
                    Tool.log(repository, "Resqml 2.0.1 base type : " + baseType2_0_1.Name);
                }

                // si je suis dans le cas d'une mesure, dans ce cas je vais devoir generer un getter pour la valeur et un getter pour l'unité de mesure
                if (Tool.isMeasureType(baseType2_2) && Tool.isMeasureType(baseType2_0_1))
                {
                    addResqml2MeasureGetter(fesapiResqml2Class, energisticsResqml2_2Attribute.Name);
                    // todo il faudrait passer l'objet attribut en paramètre
                    EA.Attribute resqml2_2UomAttribute = baseType2_2.Attributes.GetAt(0); // un peu dangereux, on suppose que c'est l'attribut "uom", car le GetByName ne peut pas etre utiliser pour une collection d'attributs
                    EA.Attribute resqml2_0_1UomAttribute = baseType2_0_1.Attributes.GetAt(0); // un peu dangereux, on suppose que c'est l'attribut "uom", car le GetByName ne peut pas etre utiliser pour une collection d'attributs
                    addResqml2EnumConversionGetter(fesapiResqml2Class, "get" + energisticsResqml2_2Attribute.Name + "Uom", energisticsResqml2_2Attribute.Name + "->uom", resqml2_2UomAttribute.Type, resqml2_0_1UomAttribute.Type);
                    addResqml2EnumGetterAsString(fesapiResqml2Class, "get" + energisticsResqml2_2Attribute.Name + "Uom", resqml2_2UomAttribute.Type);

                    Tool.log(repository, "Measure getters generated");
                }    
                else if ((baseType2_0_1.Type.Equals("Enumeration") || baseType2_0_1.Stereotype.Equals("enumeration")) && (baseType2_2.Type.Equals("Enumeration") || baseType2_2.Stereotype.Equals("enumeration")))
                {
                    addResqml2EnumConversionGetter(fesapiResqml2Class, "get" + energisticsResqml2_2Attribute.Name, energisticsResqml2_2Attribute.Name, baseType2_2.Name, baseType2_0_1.Name);
                    addResqml2EnumGetterAsString(fesapiResqml2Class, "get" + energisticsResqml2_2Attribute.Name, baseType2_2.Name);

                    Tool.log(repository, "Enum getter generated");
                }
                else if ((baseType2_0_1.Type.Equals("Enumeration") || baseType2_0_1.Stereotype.Equals("enumeration")) && baseType2_2.Name.EndsWith("Ext"))
                {
                    string baseType2_2NameWithoutSuffix = baseType2_2.Name.Remove(baseType2_2.Name.Length - 3);
                    addResqml2EnumExtConversionGetter(fesapiResqml2Class, "get" + energisticsResqml2_2Attribute.Name, energisticsResqml2_2Attribute.Name, baseType2_2NameWithoutSuffix, baseType2_0_1.Name);
                    addResqml2EnumGetterAsString(fesapiResqml2Class, "get" + energisticsResqml2_2Attribute.Name, baseType2_2NameWithoutSuffix);

                    Tool.log(repository, "Enum ext getter generated");
                }
            
            }
        }

        private void getterSetting()
        {
            Tool.log(repository, "BEGIN getterSetting...");

            Tool.log(repository, "resqml2 classes");

            // handling fesapi/resqml2 classes
            foreach (EA.Element fesapiResqml2Class in fesapiResqml2ClassList)
            {

                Tool.log(repository, "class : " + fesapiResqml2Class.Name);

                EA.Element energisticsResqml2_0_1Class = fesapiResqml2ToEnergisticsResqml2_0_1[fesapiResqml2Class];
                EA.Element energisticsResqml2_2Class = fesapiResqml2ToEnergisticsResqml2_2[fesapiResqml2Class];

                foreach (EA.Attribute energisticsResqml2_0_1Attribute in energisticsResqml2_0_1Class.Attributes)
                {
                    EA.Attribute energisticsResqml2_2Attribute = null;
                    foreach (EA.Attribute currentEnergisticsResqml2_2Attribute in energisticsResqml2_2Class.Attributes)
                    {
                        if (energisticsResqml2_0_1Attribute.Name.Equals(currentEnergisticsResqml2_2Attribute.Name))
                        {
                            energisticsResqml2_2Attribute = currentEnergisticsResqml2_2Attribute;
                            break;
                        }
                    }

                    if (energisticsResqml2_2Attribute == null)
                    {
                        break;
                    }

                    addResqml2Getter(fesapiResqml2Class, energisticsResqml2_0_1Attribute, energisticsResqml2_2Attribute);
                }
            }

            Tool.log(repository, "resqml2_0_1 classes");

            // handling fesapi/resqml2_0_1 classes
            foreach (EA.Element fesapiResqml2_0_1Class in fesapiResqml2_0_1ClassList)
            {
                EA.Element fesapiResqml2Class = fesapiResqml2ClassList.Find(c => c.Name.Equals(fesapiResqml2_0_1Class.Name));
                if (fesapiResqml2Class != null) // the class exists in resqml2
                {
                    EA.Element energisticsResqml2_0_1Class = fesapiResqml2ToEnergisticsResqml2_0_1[fesapiResqml2Class];
                    EA.Element energisticsResqml2_2Class = fesapiResqml2ToEnergisticsResqml2_2[fesapiResqml2Class];

                    // we look for attributes which are not common between the Resqml 2.0.1 and the Resqml 2.2 classes
                    foreach (EA.Attribute energisticsResqml2_0_1Attribute in energisticsResqml2_0_1Class.Attributes)
                    {
                        EA.Attribute energisticsResqml2_2Attribute = null;
                        foreach (EA.Attribute currentEnergisticsResqml2_2Attribute in energisticsResqml2_2Class.Attributes)
                        {
                            if (energisticsResqml2_0_1Attribute.Name.Equals(currentEnergisticsResqml2_2Attribute.Name))
                            {
                                energisticsResqml2_2Attribute = currentEnergisticsResqml2_2Attribute;
                                break;
                            }
                        }

                        if ((energisticsResqml2_2Attribute == null) && Tool.isBasicType(energisticsResqml2_0_1Attribute.Type))
                        {
                            addResqml2_0_1SimpleGetter(fesapiResqml2_0_1Class, energisticsResqml2_0_1Attribute.Name, energisticsResqml2_0_1Attribute.Type);
                        }
                    }
                }
                else
                {
                    EA.Element energisticsResqml2_0_1Class = fesapiResqml2_0_1toEnergisticsResqml2_0_1[fesapiResqml2_0_1Class];

                    foreach (EA.Attribute energisticsResqml2_0_1Attribute in energisticsResqml2_0_1Class.Attributes)
                    {
                        if (Tool.isBasicType(energisticsResqml2_0_1Attribute.Type))
                        {
                            addResqml2_0_1SimpleGetter(fesapiResqml2_0_1Class, energisticsResqml2_0_1Attribute.Name, energisticsResqml2_0_1Attribute.Type);
                        }
                    }
                }
            }

            Tool.log(repository, "resqml2_2 classes");

            // handling fesapi/resqml2_2 classes
            foreach (EA.Element fesapiResqml2_2Class in fesapiResqml2_2ClassList)
            {
                EA.Element fesapiResqml2Class = fesapiResqml2ClassList.Find(c => c.Name.Equals(fesapiResqml2_2Class.Name));
                if (fesapiResqml2Class != null) // the class exists in resqml2
                {
                    EA.Element energisticsResqml2_0_1Class = fesapiResqml2ToEnergisticsResqml2_0_1[fesapiResqml2Class];
                    EA.Element energisticsResqml2_2Class = fesapiResqml2ToEnergisticsResqml2_2[fesapiResqml2Class];

                    // we look for attributes which are not common between the Resqml 2.0.1 and the Resqml 2.2 classes
                    foreach (EA.Attribute energisticsResqml2_2Attribute in energisticsResqml2_2Class.Attributes)
                    {
                        EA.Attribute energisticsResqml2_0_1Attribute = null;
                        foreach (EA.Attribute currentEnergisticsResqml2_0_1Attribute in energisticsResqml2_0_1Class.Attributes)
                        {
                            if (energisticsResqml2_2Attribute.Name.Equals(currentEnergisticsResqml2_0_1Attribute.Name))
                            {
                                energisticsResqml2_0_1Attribute = currentEnergisticsResqml2_0_1Attribute;
                                break;
                            }
                        }

                        if ((energisticsResqml2_0_1Attribute == null) && Tool.isBasicType(energisticsResqml2_2Attribute.Type))
                        {
                            addResqml2_0_1SimpleGetter(fesapiResqml2_2Class, energisticsResqml2_2Attribute.Name, energisticsResqml2_2Attribute.Type);
                        }
                    }
                }
                else
                {
                    EA.Element energisticsResqml2_2Class = fesapiResqml2_2toEnergisticsResqml2_2[fesapiResqml2_2Class];

                    foreach (EA.Attribute energisticsResqml2_2Attribute in energisticsResqml2_2Class.Attributes)
                    {
                        if (Tool.isBasicType(energisticsResqml2_2Attribute.Type))
                        {
                            addResqml2_2SimpleGetter(fesapiResqml2_2Class, energisticsResqml2_2Attribute.Name, energisticsResqml2_2Attribute.Type);
                        }
                    }
                }
            }

            Tool.log(repository, "END getterSetting");
        }

        #endregion

    }
}
