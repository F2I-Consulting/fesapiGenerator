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
                    Tool.log(repository, className);

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

        #region private

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
            if (baseClassPackageName != childclassPackageName) // if the fesapi class and fesapi base class belong to a different
            {
                // package, we need to provide the relative path of the header file
                includeTagValue += baseClassPackageName + "/";
            }
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
            // handling fesapi/resqml2 classes
            foreach (EA.Element resqml2Class in fesapiResqml2ClassList)
            {
                EA.Element energisticsResqml2_0_1Class = fesapiResqml2ToEnergisticsResqml2_0_1[resqml2Class];
                EA.Element energisticsResqml2_2Class = fesapiResqml2ToEnergisticsResqml2_2[resqml2Class];

                if ((energisticsResqml2_0_1Class.BaseClasses.Count == 0) && (energisticsResqml2_2Class.BaseClasses.Count == 0))
                    continue; // should never happened 

                // first, we look for fesapi/resqml2 classes inheriting from fesapi/common/AbstracObject
                if (energisticsResqml2_0_1Class.BaseClasses.GetAt(0).Name.Equals("AbstractResqmlDataObject") &&
                    energisticsResqml2_2Class.BaseClasses.GetAt(0).Name.Equals("AbstractObject"))
                {
                    addGeneralizationConnector(resqml2Class, abstractObject);

                    //EA.Connector inheritConnector = resqml2Class.Connectors.AddNew("", "Generalization");
                    //inheritConnector.SupplierID = abstractObject.ElementID;
                    //if (!(inheritConnector.Update()))
                    //{
                    //    Tool.showMessageBox(repository, inheritConnector.GetLastError());
                    //}
                    //resqml2Class.Refresh();
                }
                else // then, we look for fesapi/resqml2 classes inheriting from fesapi/resqml2 classes
                {
                    // since fesapi/resqml2 classes exist in both Resqml 2.0.1 and Resqml 2.2, we only look at Resqml 2.2 mapped class
                    EA.Element resqml2ParentClass = fesapiResqml2ClassList.Find(c => energisticsResqml2_2Class.BaseClasses.GetAt(0).Name.Equals(c.Name));
                    if (resqml2ParentClass != null)
                    {
                        addGeneralizationConnector(resqml2Class, resqml2ParentClass);

                        //EA.Connector inheritConnector = resqml2Class.Connectors.AddNew("", "Generalization");
                        //inheritConnector.SupplierID = resqml2ParentClass.ElementID;
                        //if (!(inheritConnector.Update()))
                        //{
                        //    Tool.showMessageBox(repository, inheritConnector.GetLastError());
                        //}
                        //resqml2Class.Refresh();
                    }
                }
            }

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

                    //EA.Connector inheritConnector = resqml2_0_1Class.Connectors.AddNew("", "Generalization");
                    //inheritConnector.SupplierID = abstractObject.ElementID;
                }
                else
                {
                    // then, we look if the parent class is in fesapi/resqml2_0_1 
                    EA.Element resqml2_0_1ParentClass = fesapiResqml2_0_1ClassList.Find(c => energisticsResqml2_0_1Class.BaseClasses.GetAt(0).Name.Equals(c.Name));
                    if (resqml2_0_1ParentClass != null)
                    {
                        addGeneralizationConnector(resqml2_0_1Class, resqml2_0_1ParentClass);

                        //EA.Connector inheritConnector = resqml2_0_1Class.Connectors.AddNew("", "Generalization");
                        //inheritConnector.SupplierID = resqml2_0_1ParentClass.ElementID;
                        //if (!(inheritConnector.Update()))
                        //{
                        //    Tool.showMessageBox(repository, inheritConnector.GetLastError());
                        //}
                        //resqml2_0_1Class.Refresh();
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

                            //EA.Connector inheritConnector = resqml2_0_1Class.Connectors.AddNew("", "Generalization");
                            //inheritConnector.SupplierID = resqml2ParentClass.ElementID;
                            //if (!(inheritConnector.Update()))
                            //{
                            //    Tool.showMessageBox(repository, inheritConnector.GetLastError());
                            //}
                            //resqml2_0_1Class.Refresh();
                        }
                    }

                }
            }

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

                    //EA.Connector inheritConnector = resqml2_2Class.Connectors.AddNew("", "Generalization");
                    //inheritConnector.SupplierID = abstractObject.ElementID;
                }
                else
                {
                    // then, we look if the parent class is in fesapi/resqml2_2 
                    EA.Element resqml2_2ParentClass = fesapiResqml2_2ClassList.Find(c => energisticsResqml2_2Class.BaseClasses.GetAt(0).Name.Equals(c.Name));
                    if (resqml2_2ParentClass != null)
                    {
                        addGeneralizationConnector(resqml2_2Class, resqml2_2ParentClass);

                        //EA.Connector inheritConnector = resqml2_2Class.Connectors.AddNew("", "Generalization");
                        //inheritConnector.SupplierID = resqml2_2ParentClass.ElementID;
                        //if (!(inheritConnector.Update()))
                        //{
                        //    Tool.showMessageBox(repository, inheritConnector.GetLastError());
                        //}
                        //resqml2_2Class.Refresh();
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

                            //EA.Connector inheritConnector = resqml2_2Class.Connectors.AddNew("", "Generalization");
                            //inheritConnector.SupplierID = resqml2ParentClass.ElementID;
                            //if (!(inheritConnector.Update()))
                            //{
                            //    Tool.showMessageBox(repository, inheritConnector.GetLastError());
                            //}
                            //resqml2_2Class.Refresh();
                        }
                    }

                }
            }
        }

        private EA.Method addPartialTransfertConstructor(EA.Element fesapiClass, string gsoapPrefix)
        {
            // adding a new partial transfert constructor, basically a method named whith the Energistics class name
            EA.Method constructor = fesapiClass.Methods.AddNew(fesapiClass.Name, "");
            constructor.Code = "";
            constructor.Notes = "Only to be used in partial transfer context";
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
            constructor.TaggedValues.Refresh();

            // adding the partial transfert parameter
            EA.Parameter parameter = constructor.Parameters.AddNew("partialObject", gsoapPrefix + "DataObjectReference* ");
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

        private EA.Method addDeserializationConstructor(EA.Element fesapiClass, string gsoapPrefix)
        {
            // adding a new deserialization constructor, basically a method named whith the Energistics class name
            EA.Method constructor = fesapiClass.Methods.AddNew(fesapiClass.Name, "");
            constructor.Code = "";
            constructor.Notes = "Creates an instance of this class by wrapping a gsoap instance.";
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
            // handling fesapi/resqml2 classes
            foreach (EA.Element resqml2Class in fesapiResqml2ClassList)
            {
                addPartialTransfertConstructor(resqml2Class, Constants.eml20GsoapPrefix);
                addPartialTransfertConstructor(resqml2Class, Constants.eml22GsoapPrefix);
                addDeserializationConstructor(resqml2Class, Constants.resqml2_0_1GsoapPrefix);
                addDeserializationConstructor(resqml2Class, Constants.resqml2_2GsoapPrefix);
            }

            // handling fesapi/resqml2_0_1 classes
            foreach (EA.Element resqml2_0_1Class in fesapiResqml2_0_1ClassList)
            {
                addPartialTransfertConstructor(resqml2_0_1Class, Constants.eml20GsoapPrefix);
                addDeserializationConstructor(resqml2_0_1Class, Constants.resqml2_0_1GsoapPrefix);
            }

            // handling fesapi/resqml2_2 classes
            foreach (EA.Element resqml2_2Class in fesapiResqml2_2ClassList)
            {
                addPartialTransfertConstructor(resqml2_2Class, Constants.eml22GsoapPrefix);
                addDeserializationConstructor(resqml2_2Class, Constants.resqml2_2GsoapPrefix);
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
            xmlTagAttribute.Default = fesapiClass.Name;
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

        private EA.Method addSimpleGetter(EA.Element fesapiClass, string attributeName, string attributeType)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load("C:\\Users\\Mathieu Poudret\\Documents\\Projets 2017\\fesapi generator\\fesapiGenConf.xml");
            string xmlResqml2_0_1GsoapProxy = xmlDoc.DocumentElement.SelectSingleNode("/FesapiGenConf/Constants/Resqml2_0_1GsoapProxy").Attributes["value"].Value;
            string xmlResqml2_0_1GSoapPrefix = xmlDoc.DocumentElement.SelectSingleNode("/FesapiGenConf/Constants/Resqml2_0_1GsoapPrefix").Attributes["value"].Value;
            string xmlResqml2_2GsoapProxy = xmlDoc.DocumentElement.SelectSingleNode("/FesapiGenConf/Constants/Resqml2_2GsoapProxy").Attributes["value"].Value;
            string xmlResqml2_2GSoapPrefix = xmlDoc.DocumentElement.SelectSingleNode("/FesapiGenConf/Constants/Resqml2_2GsoapPrefix").Attributes["value"].Value;
            string xmlAttributeAccess = xmlDoc.DocumentElement.SelectSingleNode("/FesapiGenConf/Expressions/AttributeAccess").Attributes["value"].Value;

            EA.Method getter = fesapiClass.Methods.AddNew("get" + attributeName, attributeType);
            getter.Code = "if (" + xmlResqml2_0_1GsoapProxy + "!= nullptr)\n";
            getter.Code += "{\n";
            getter.Code += "\treturn " + xmlAttributeAccess.Replace("#PREFIX", xmlResqml2_0_1GSoapPrefix).Replace("#CLASS", fesapiClass.Name).Replace("#PROXY", xmlResqml2_0_1GsoapProxy).Replace("#ATTRIBUTE", attributeName) + "\n";
            getter.Code += "}\n";
            getter.Code += "else if (" + xmlResqml2_2GsoapProxy + "!= nullptr)\n";
            getter.Code += "{\n";
            getter.Code += "\treturn " + xmlAttributeAccess.Replace("#PREFIX", xmlResqml2_2GSoapPrefix).Replace("#CLASS", fesapiClass.Name).Replace("#PROXY", xmlResqml2_2GsoapProxy).Replace("#ATTRIBUTE", attributeName) + "\n";
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

            EA.MethodTag bodyLocationTag = getter.TaggedValues.AddNew("bodyLocation", "classDec");// "classBody");
            if (!(bodyLocationTag.Update()))
            {
                Tool.showMessageBox(repository, bodyLocationTag.GetLastError());
            }
            getter.TaggedValues.Refresh();

            return getter;
        }

        private void getterSetting()
        {
            // handling fesapi/resqml2 classes
            foreach (EA.Element fesapiResqml2Class in fesapiResqml2ClassList)
            {
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

                    if ((energisticsResqml2_0_1Attribute.Type.Equals(energisticsResqml2_2Attribute.Type)) && Tool.isBasicType(energisticsResqml2_2Attribute.Type))
                    {
                        addSimpleGetter(fesapiResqml2Class, energisticsResqml2_0_1Attribute.Name, energisticsResqml2_0_1Attribute.Type);
                    }
                }
            }
        }

        #endregion

    }
}
