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

namespace fesapiGenerator
{
    /// <summary>
    /// Class responsible for updating the Energistics model according to
    /// the fesapi input model
    /// </summary>
    public class EnergisticsModelUpdate
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
        /// Energistics resqml model;
        /// </summary>
        private EA.Package resqmlModel;

        /// <summary>
        /// Fesapi model
        /// </summary>
        private EA.Package fesapiModel;

        /// <summary>
        /// List of Energistics model (common and resqml) classes.
        /// </summary>
        private List<EA.Element> energisticsClassList = null;

        /// <summary>
        /// List of fesapi classes.
        /// </summary>
        private List<EA.Element> fesapiClassList = null;

        #endregion

        #region constructor

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="repository">EA repository</param>
        /// <param name="commonModel">Energistics common model</param>
        /// <param name="resqmlModel">Energistics resqml model</param>
        /// <param name="fesapiModel">fesapi model</param>
        public EnergisticsModelUpdate(EA.Repository repository, EA.Package commonModel, EA.Package resqmlModel, EA.Package fesapiModel)
        {
            this.repository = repository;
            this.commonModel = commonModel;
            this.resqmlModel = resqmlModel;
            this.fesapiModel = fesapiModel;

            // getting the Energistics model classes
            energisticsClassList = new List<EA.Element>();
            Tool.fillElementList(commonModel, energisticsClassList);
            Tool.fillElementList(resqmlModel, energisticsClassList);

            // getting the fesapi classes
            fesapiClassList = new List<EA.Element>();
            Tool.fillElementList(fesapiModel, fesapiClassList);
        }

        #endregion

        #region public methods

        /// <summary>
        ///  Updated the energistics model (common and resqml model) according to the input fesapi model.
        /// </summary>
        public void updateEnergisticsModel()
        {
            // looking for:
            // - fesapi classes that exists in the energistics model (according to their name). For
            //   that purpose, toUpdateClassF2E makes a correspondance between a fesapi class (key) and
            //   corresponding Energistics class (value). These classes will then be updated into
            //   the energistics model
            // - fesapi classes that does not exist in the Energistics model. These classes are collected
            //   into toAddClassList. These classes will then be added to the Energistics model
            Dictionary<EA.Element, EA.Element> toUpdateClassF2E = new Dictionary<EA.Element, EA.Element>();
            List<EA.Element> toAddClassList = new List<EA.Element>();
            foreach (EA.Element fesapiClass in fesapiClassList)
            {
                EA.Element energisticsClass = energisticsClassList.Find(c => c.Name.Equals(fesapiClass.Name));
                if (energisticsClass != null)
                    toUpdateClassF2E.Add(fesapiClass, energisticsClass);
                else
                    toAddClassList.Add(fesapiClass);
            }

            // console output
            Tool.log(repository, "classes to update:");
            foreach (KeyValuePair<EA.Element, EA.Element> entry in toUpdateClassF2E)
                Tool.log(repository, "   " + entry.Value.Name);
            Tool.log(repository, "classes to add:");
            foreach (EA.Element c in toAddClassList)
                Tool.log(repository, "   " + c.Name);

            // handling classes that exist into the Energistics model
            foreach (KeyValuePair<EA.Element, EA.Element> entry in toUpdateClassF2E)
            {
                EA.Element fesapiClass = entry.Key;
                EA.Element energisticsClass = entry.Value;

                // adding a fesapiGeneration tag to true
                EA.TaggedValue t = energisticsClass.TaggedValues.AddNew(Constants.fesapiGenerationTagName, "true");
                if (!(t.Update()))
                {
                    Tool.showMessageBox(repository, t.GetLastError());
                    continue;
                }
                energisticsClass.TaggedValues.Refresh();

                // adding a fesapiNamespace tag. The tag value is set to the name of 
                // package containing the fesapi class. This tag will be used to determine:
                // - the name space of the generated class
                // - the output directory of the generated class
                EA.Package p = repository.GetPackageByID(fesapiClass.PackageID);
                t = energisticsClass.TaggedValues.AddNew(Constants.fesapiNamespaceTagName, p.Name);
                if (!(t.Update()))
                {
                    Tool.showMessageBox(repository, t.GetLastError());
                    continue;
                }
                energisticsClass.TaggedValues.Refresh();

                // updating the ENergistics class according to the corresponding
                // fesapi class
                updateEnergisticsClass(energisticsClass, fesapiClass);
            }

            // handling classes that does not exist into the Energistics model
            foreach (EA.Element c in toAddClassList)
            {
                // getting the package carrying the fesapi class
                EA.Package sourcePackagePackage = repository.GetPackageByID(c.PackageID);

                // getting the package where to create the corresponding Energistics class
                EA.Package targetPackage = null;
                if (sourcePackagePackage.Name.Contains("common"))
                    targetPackage = Tool.getOrCreatePackage(repository, commonModel, "fesapi");
                else if (sourcePackagePackage.Name.Contains("resqml"))
                    targetPackage = Tool.getOrCreatePackage(repository, resqmlModel, "fesapi");
                if (targetPackage == null)
                {
                    Tool.showMessageBox(repository, "The fesapi class " + c.Name + " is not handled. It will be skipped!");
                    continue;
                }

                // creating the new class
                EA.Element newClass = Tool.copyElement(repository, c, targetPackage);
                if (newClass == null)
                {
                    Tool.showMessageBox(repository, "Not able to create a " + c.Name + " class in the Energistics model");
                    continue;
                }

                // adding a fesapiGeneration tag to true
                EA.TaggedValue t = newClass.TaggedValues.AddNew(Constants.fesapiGenerationTagName, "true");
                if (!(t.Update()))
                {
                    Tool.showMessageBox(repository, t.GetLastError());
                    continue;
                }
                newClass.TaggedValues.Refresh();

                // adding a fesapiNamespace tag. The tag value is set to the name of 
                // package containing the fesapi class. This tag will be used to determine:
                // - the name space of the generated class
                // - the output directory of the generated class
                t = newClass.TaggedValues.AddNew(Constants.fesapiNamespaceTagName, sourcePackagePackage.Name);
                if (!(t.Update()))
                {
                    Tool.showMessageBox(repository, t.GetLastError());
                    continue;
                }
                newClass.TaggedValues.Refresh();
            }

            // looking in the Energistics common model for a newly created EpcDocument class
            // if such class exists, we create its easy access vector members
            EA.Package commonFesapiPackage;
            try
            {
                commonFesapiPackage = commonModel.Packages.GetByName("fesapi");
            }
            catch (Exception)
            {
                commonFesapiPackage = null;
            }
            if (commonFesapiPackage != null)
            {
                EA.Element epcDocumentClass = commonFesapiPackage.Elements.GetByName("EpcDocument");
                if (epcDocumentClass != null)
                {
                    createEpcDocumentEasyAccessVector(epcDocumentClass, toUpdateClassF2E);
                }
            }

            // make sure the model view is up to date in the Enterprise Architect GUI
            repository.RefreshModelView(0);
        }

        #endregion

        #region private methods

        /// <summary>
        /// Update an Energistics class according to its corresponding fesapi class
        /// </summary>
        /// <param name="energisticsClass">The Energistics class to update</param>
        /// <param name="fesapiClass">THe corresponding fesapi class</param>
        private void updateEnergisticsClass(EA.Element energisticsClass, EA.Element fesapiClass)
        {
            // tagging the Energistics class in order to properly generate:
            /// - its base class #include directive
            /// - its base class namespace
            EA.Element fesapiBaseClass = inheritanceTagging(energisticsClass, fesapiClass);

            // adding a constructor based on the gSOAP proxy
            addFromGsoapConstructor(energisticsClass, fesapiBaseClass);

            // potentially add an XML_TAG attribute with its getter
            addXmlTagAttributeWithGetter(energisticsClass, fesapiClass);

            // copying the content of the fesapi class into the Energistics class.
            copyClassContent(fesapiClass, energisticsClass);
        }

        /// <summary>
        /// This methods tags a given Energistics class in order to properly generate:
        /// - its base class #include directive
        /// - its base class namespace
        /// This methods produces no effect if the Energistics class does not inherit or if
        /// its base class have no correspondinf fesapi class
        /// </summary>
        /// <param name="energisticsClass">An Energistics class</param>
        /// <param name="fesapiClass">Its corresponding fesapi class</param>
        /// <returns>the fesapi class corresponding to the Energistics base class; null if the
        /// Energistics class does not inherit of if its base class
        /// have no corresponding fesapi class</returns>
        private EA.Element inheritanceTagging(EA.Element energisticsClass, EA.Element fesapiClass)
        {
            EA.Element fesapiBaseClass = null;

            // we look for the fesapi class corresponding to the base class
            if (energisticsClass.BaseClasses.Count != 0)
            {
                // important note: we assume that energistics class inherits from 1 or 0 base class
                EA.Element energisticsBaseClass = energisticsClass.BaseClasses.GetAt(0);

                fesapiBaseClass = fesapiClassList.Find(c => c.Name.Equals(energisticsBaseClass.Name));
            }

            if (fesapiBaseClass != null)
            {
                // we set up the #include directive to be copied in the generated code
                string fesapiBaseClassPackageName = repository.GetPackageByID(fesapiBaseClass.PackageID).Name;
                string fesapiClassPackageName = repository.GetPackageByID(fesapiClass.PackageID).Name;
                string fesapiIncludeTagValue = "";
                if (fesapiBaseClassPackageName != fesapiClassPackageName) // if the fesapi class and fesapi base class belong to a different
                    // package, we need to provide the relative path of the header file
                    fesapiIncludeTagValue += fesapiBaseClassPackageName + "/";
                fesapiIncludeTagValue += fesapiBaseClass.Name + ".h";

                // tagging the energistics class with the #include directive
                EA.TaggedValue fesapiIncludeTag = energisticsClass.TaggedValues.AddNew(Constants.fesapiBaseClassIncludeTagName, fesapiIncludeTagValue);
                if (!(fesapiIncludeTag.Update()))
                {
                    Tool.showMessageBox(repository, fesapiIncludeTag.GetLastError());
                }

                // tagging the energistics class with the fesapi base class namespace (that is to say the name of its parent package)
                EA.TaggedValue fesapiBaseClassNamespaceTag = energisticsClass.TaggedValues.AddNew("fesapiBaseClassNamespace", fesapiBaseClassPackageName);
                if (!(fesapiBaseClassNamespaceTag.Update()))
                {
                    Tool.showMessageBox(repository, fesapiBaseClassNamespaceTag.GetLastError());
                }

                energisticsClass.TaggedValues.Refresh();
            }

            return fesapiBaseClass;
        }

        /// <summary>
        /// Adds a constructor based on the gSOAP proxy to a given Energistics class
        /// </summary>
        /// <param name="energisticsClass">An Energistics class</param>
        /// <param name="fesapiBaseClass">Correspondinf fesapi base class if exists else null</param>
        private void addFromGsoapConstructor(EA.Element energisticsClass, EA.Element fesapiBaseClass)
        {
            // adding a new constructor, basically a method named whith the Energistics class name
            EA.Method fesapiConstructor = energisticsClass.Methods.AddNew(energisticsClass.Name, "");
            if (!(fesapiConstructor.Update()))
            {
                Tool.showMessageBox(repository, fesapiConstructor.GetLastError());
            }
            energisticsClass.Methods.Refresh();

            // adding the gSOAP proxy parameter
            string fesapiConstructorParamType = "gsoap_resqml2_0_1::resqml2__" + energisticsClass.Name + "*";
            EA.Parameter fesapiConstructParam = fesapiConstructor.Parameters.AddNew("fromGsoap", fesapiConstructorParamType);
            if (!(fesapiConstructParam.Update()))
            {
                Tool.showMessageBox(repository, fesapiConstructParam.GetLastError());
            }
            fesapiConstructor.Parameters.Refresh();

            // adding a tag sepcifying that the body will be located into the class declaration
            EA.MethodTag fesapiConstructorBodyLocationTag = fesapiConstructor.TaggedValues.AddNew("bodyLocation", "classDec");
            if (!(fesapiConstructorBodyLocationTag.Update()))
            {
                Tool.showMessageBox(repository, fesapiConstructorBodyLocationTag.GetLastError());
            }

            // if there exists a correspondinf fesapi base class, we add the proper initialization directive
            if (fesapiBaseClass != null)
            {
                string initializerTagValue = fesapiBaseClass.Name + "(fromGsoap)";
                EA.MethodTag initializerTag = fesapiConstructor.TaggedValues.AddNew("initializer", initializerTagValue);
                if (!(initializerTag.Update()))
                {
                    Tool.showMessageBox(repository, initializerTag.GetLastError());
                }
            }
            fesapiConstructor.TaggedValues.Refresh();

            // adding constructor notes
            fesapiConstructor.Notes = "Creates an instance of this class by wrapping a gsoap instance.";
            if (!(fesapiConstructor.Update()))
            {
                Tool.showMessageBox(repository, fesapiConstructor.GetLastError());
            }
        }

        /// <summary>
        /// Add an XML_TAG attribute with its getter to a given Energistics class. This method
        /// has no effect if the Energistics class is abstract or if the corresponding fesapi class
        /// is tagged such that no XML_TAG should be generated
        /// </summary>
        /// <param name="energisticsClass">An Energistics class</param>
        /// <param name="fesapiClass">Its corresponding fesapi class</param>
        private void addXmlTagAttributeWithGetter(EA.Element energisticsClass, EA.Element fesapiClass)
        {
            // we look for a fesapi class tag telling that no XML_TAG attribute should be generated
            string generateXmlTag = "true";
            EA.TaggedValue generateXmlTagTag = fesapiClass.TaggedValues.GetByName(Constants.fesapiGenerateXmlTagTagName);
            if (generateXmlTagTag != null)
                generateXmlTag = generateXmlTagTag.Value;

            if (!(fesapiClass.Name.Contains("Abstract")) && generateXmlTag == "true") // impotant note: we assume that a class is
                                                                                      // abstract if it contains "Abstract" in its
                                                                                      // name
            {
                // we add an XML_TAG attribute
                EA.Attribute xmlTagAttribute = energisticsClass.Attributes.AddNew("XML_TAG", "char*");
                xmlTagAttribute.IsStatic = true;
                xmlTagAttribute.IsConst = true;
                if (!(xmlTagAttribute.Update()))
                {
                    Tool.showMessageBox(repository, xmlTagAttribute.GetLastError());
                }
                energisticsClass.Attributes.Refresh();

                // thanks to a tag, we tells that the XML_TAG attribute should be generated during the code generation.
                // Keep in mind that Energistics class attributes are not generated in fesapi by default since most of them
                // relies on gSOAP proxies attributes
                EA.AttributeTag generationTag = xmlTagAttribute.TaggedValues.AddNew(Constants.fesapiGenerationTagName, "true");
                if (!(generationTag.Update()))
                {
                    Tool.showMessageBox(repository, generationTag.GetLastError());
                }

                // thanks to a tag, we tells that no getter should be automatically generated during the transformation
                // of the Energistics model into a C++ model
                EA.AttributeTag getterGenerationTag = xmlTagAttribute.TaggedValues.AddNew(Constants.fesapiGetterGenerationTagName, "false");
                if (!(getterGenerationTag.Update()))
                {
                    Tool.showMessageBox(repository, getterGenerationTag.GetLastError());
                }

                // add the XML_TAG attributr getter
                EA.Method getXmlTagMethod = energisticsClass.Methods.AddNew("getXmlTag", "std::string");
                getXmlTagMethod.Stereotype = "const";
                getXmlTagMethod.Code = "return XML_TAG;";
                getXmlTagMethod.Abstract = true;
                if (!(getXmlTagMethod.Update()))
                {
                    Tool.showMessageBox(repository, getXmlTagMethod.GetLastError());
                }
                energisticsClass.Methods.Refresh();

                // tag the getter in order for its body to be generated into the class declaration
                EA.MethodTag getXmlTagMethodBodyLocationTag = getXmlTagMethod.TaggedValues.AddNew("bodyLocation", "classDec");
                if (!(getXmlTagMethodBodyLocationTag.Update()))
                {
                    Tool.showMessageBox(repository, getXmlTagMethodBodyLocationTag.GetLastError());
                }
                getXmlTagMethod.TaggedValues.Refresh();
            }
        }

        /// <summary>
        /// This method basically copy the content of a fesapi class into an Energistics class.
        /// Attributes, methods, tags and notes are handled.
        /// Important notes: 
        /// - common attributes and tags (according to their name) existing content will be erased
        /// - existing notes will be erased during this process
        /// </summary>
        /// <param name="fesapiClass">A source fesapi class</param>
        /// <param name="energisticsClass">A target Energistics class</param>
        private void copyClassContent(EA.Element fesapiClass, EA.Element energisticsClass)
        {
            // -------------------
            // -------------------
            // attributes handling

            // for each fesapi attribute, if a corresponding Energistics attritute exists
            // (that is to say if the Energistics class contains an attribute with the same name)
            // we copy the fesapi attribute content into the Energistics attribute content else we create from scratch
            // an Energistics attribute
            foreach (EA.Attribute fesapiAttribute in fesapiClass.Attributes)
            {
                EA.Attribute correspondingEnergisticsAttribute = null;
                foreach (EA.Attribute a in energisticsClass.Attributes)
                {
                    if (a.Name == fesapiAttribute.Name)
                    {
                        correspondingEnergisticsAttribute = a;
                        break;
                    }
                }

                // if there is no corresponding target attribute, we create one
                if (correspondingEnergisticsAttribute == null)
                {
                    correspondingEnergisticsAttribute = energisticsClass.Attributes.AddNew(fesapiAttribute.Name, fesapiAttribute.Type);
                    if (!(correspondingEnergisticsAttribute.Update()))
                    {
                        Tool.showMessageBox(repository, correspondingEnergisticsAttribute.GetLastError());
                        continue;
                    }
                    energisticsClass.Attributes.Refresh();
                }

                correspondingEnergisticsAttribute.Alias = fesapiAttribute.Alias;
                correspondingEnergisticsAttribute.AllowDuplicates = fesapiAttribute.AllowDuplicates;
                correspondingEnergisticsAttribute.ClassifierID = fesapiAttribute.ClassifierID;
                correspondingEnergisticsAttribute.Container = fesapiAttribute.Container;
                correspondingEnergisticsAttribute.Containment = fesapiAttribute.Containment;
                correspondingEnergisticsAttribute.Default = fesapiAttribute.Default;
                correspondingEnergisticsAttribute.IsCollection = fesapiAttribute.IsCollection;
                correspondingEnergisticsAttribute.IsConst = fesapiAttribute.IsConst;
                correspondingEnergisticsAttribute.IsDerived = fesapiAttribute.IsDerived;
                correspondingEnergisticsAttribute.IsID = fesapiAttribute.IsID;
                correspondingEnergisticsAttribute.IsOrdered = fesapiAttribute.IsOrdered;
                correspondingEnergisticsAttribute.IsStatic = fesapiAttribute.IsStatic;
                correspondingEnergisticsAttribute.Length = fesapiAttribute.Length;
                correspondingEnergisticsAttribute.LowerBound = fesapiAttribute.LowerBound;
                correspondingEnergisticsAttribute.Notes = fesapiAttribute.Notes;
                correspondingEnergisticsAttribute.Precision = fesapiAttribute.Precision;
                correspondingEnergisticsAttribute.RedefinedProperty = fesapiAttribute.RedefinedProperty;
                correspondingEnergisticsAttribute.Scale = fesapiAttribute.Scale;
                correspondingEnergisticsAttribute.Stereotype = fesapiAttribute.Stereotype;
                correspondingEnergisticsAttribute.StereotypeEx = fesapiAttribute.StereotypeEx;
                correspondingEnergisticsAttribute.Style = fesapiAttribute.Style;
                correspondingEnergisticsAttribute.StyleEx = fesapiAttribute.StyleEx;
                correspondingEnergisticsAttribute.SubsettedProperty = fesapiAttribute.SubsettedProperty;
                correspondingEnergisticsAttribute.Type = fesapiAttribute.Type;
                correspondingEnergisticsAttribute.UpperBound = fesapiAttribute.UpperBound;
                correspondingEnergisticsAttribute.Visibility = fesapiAttribute.Visibility;
                if (!(correspondingEnergisticsAttribute.Update()))
                {
                    Tool.showMessageBox(repository, correspondingEnergisticsAttribute.GetLastError());
                    continue;
                }

                foreach (EA.AttributeTag fesapiAttributeTag in fesapiAttribute.TaggedValues)
                {
                    EA.AttributeTag energisticsAttributeTag = correspondingEnergisticsAttribute.TaggedValues.AddNew(fesapiAttributeTag.Name, fesapiAttributeTag.Value);
                    if (!(energisticsAttributeTag.Update()))
                    {
                        Tool.showMessageBox(repository, energisticsAttributeTag.GetLastError());
                    }
                }
                correspondingEnergisticsAttribute.TaggedValues.Refresh();
            }

            // ----------------
            // ----------------
            // methods handling

            // we assume that the Energistics model does not contain methods. So, for each fesapi class method
            // a copy is created into the energistics class
            foreach (EA.Method fesapiMethod in fesapiClass.Methods)
            {
                EA.Method energisticsMethod = energisticsClass.Methods.AddNew(fesapiMethod.Name, fesapiMethod.ReturnType);
                energisticsMethod.Abstract = fesapiMethod.Abstract;
                energisticsMethod.Behavior = fesapiMethod.Behavior;
                energisticsMethod.ClassifierID = fesapiMethod.ClassifierID;
                energisticsMethod.Code = fesapiMethod.Code;
                energisticsMethod.Concurrency = fesapiMethod.Concurrency;
                energisticsMethod.IsConst = fesapiMethod.IsConst;
                energisticsMethod.IsLeaf = fesapiMethod.IsLeaf;
                energisticsMethod.IsPure = fesapiMethod.IsPure;
                energisticsMethod.IsQuery = fesapiMethod.IsQuery;
                energisticsMethod.IsRoot = fesapiMethod.IsRoot;
                energisticsMethod.IsStatic = fesapiMethod.IsStatic;
                energisticsMethod.IsSynchronized = fesapiMethod.IsSynchronized;
                energisticsMethod.Notes = fesapiMethod.Notes;
                energisticsMethod.ReturnIsArray = fesapiMethod.ReturnIsArray;
                energisticsMethod.StateFlags = fesapiMethod.StateFlags;
                energisticsMethod.Stereotype = fesapiMethod.Stereotype;
                energisticsMethod.StereotypeEx = fesapiMethod.StereotypeEx;
                energisticsMethod.Style = fesapiMethod.Style;
                energisticsMethod.StyleEx = fesapiMethod.StyleEx;
                energisticsMethod.Throws = fesapiMethod.Throws;
                energisticsMethod.Visibility = fesapiMethod.Visibility;
                if (!(energisticsMethod.Update()))
                {
                    Tool.showMessageBox(repository, energisticsMethod.GetLastError());
                }
                energisticsClass.Methods.Refresh();

                foreach (EA.MethodTag fesapiMethodTag in fesapiMethod.TaggedValues)
                {
                    EA.MethodTag energisticsMethodTag = energisticsMethod.TaggedValues.AddNew(fesapiMethodTag.Name, fesapiMethodTag.Value);
                    if (!(energisticsMethodTag.Update()))
                    {
                        Tool.showMessageBox(repository, energisticsMethodTag.GetLastError());
                    }
                }
                energisticsMethod.TaggedValues.Refresh();

                foreach (EA.Parameter fesapiMethodParameter in fesapiMethod.Parameters)
                {
                    EA.Parameter energisticsMethodParameter = energisticsMethod.Parameters.AddNew(fesapiMethodParameter.Name, fesapiMethodParameter.Type);
                    energisticsMethodParameter.Alias = fesapiMethodParameter.Alias;
                    energisticsMethodParameter.ClassifierID = fesapiMethodParameter.ClassifierID;
                    energisticsMethodParameter.Default = fesapiMethodParameter.Default;
                    energisticsMethodParameter.IsConst = fesapiMethodParameter.IsConst;
                    energisticsMethodParameter.Kind = fesapiMethodParameter.Kind;
                    energisticsMethodParameter.Notes = fesapiMethodParameter.Notes;
                    energisticsMethodParameter.Position = fesapiMethodParameter.Position;
                    energisticsMethodParameter.Stereotype = fesapiMethodParameter.Stereotype;
                    energisticsMethodParameter.StereotypeEx = fesapiMethodParameter.StereotypeEx;
                    energisticsMethodParameter.Style = fesapiMethodParameter.Style;
                    energisticsMethodParameter.StyleEx = fesapiMethodParameter.StyleEx;

                    if (!(energisticsMethodParameter.Update()))
                    {
                        Tool.showMessageBox(repository, energisticsMethodParameter.GetLastError());
                    }
                }
                energisticsMethod.Parameters.Refresh();
            }

            // -------------
            // -------------
            // tags handling

            foreach (EA.TaggedValue fesapiTag in fesapiClass.TaggedValues)
            {
                EA.TaggedValue energisticsTag = energisticsClass.TaggedValues.AddNew(fesapiTag.Name, fesapiTag.Value);
                if (!(energisticsTag.Update()))
                {
                    Tool.showMessageBox(repository, energisticsTag.GetLastError());
                }
                energisticsClass.TaggedValues.Refresh();
            }

            // --------------
            // --------------
            // notes handling
            
            // important note: Energistics class notes are deleted during this copy
            if (fesapiClass.Notes != "")
            {
                energisticsClass.Notes = fesapiClass.Notes;
                if (!(energisticsClass.Update()))
                {
                    Tool.showMessageBox(repository, energisticsClass.GetLastError());
                }
            }

            energisticsClass.Refresh();
        }

        /// <summary>
        /// This methods create some easy access vector attributes into an Energistics EpcDocument
        /// class. These vector are usefull to access top level Resqml objects from one EpcDocument.
        /// It also provides corresponding accessors and forward declarations.
        /// </summary>
        /// <param name="energisticsEpcDocument">An Energistics EpcDocument class</param>
        /// <param name="toUpdateClassF2E">A dictionary mapping Fesapi classes with their corresponding Energistics classes</param>
        private void createEpcDocumentEasyAccessVector(EA.Element energisticsEpcDocument, 
            Dictionary<EA.Element, EA.Element> toUpdateClassF2E)
        {
            // we create a dictionry for mapping a given fesapi namespace with corresponding fesapi class.
            // Usefull for handling EpcDocument forward declarations
            Dictionary<string, List<string>> importNamespace = new Dictionary<string, List<string>>();

            foreach (KeyValuePair<EA.Element, EA.Element> entry in toUpdateClassF2E)
            {
                EA.Element energisticsClass = entry.Value;

                // we assume that we do not want to provide easy access vectors to access abstract classes from
                // EpcDocument
                if (energisticsClass.Name.Contains("Abstract"))
                    continue;

                // getting the fesapi namespace of the current non-abstract Energistics class. It exists since
                // the EpcDocument easy access vectors creation occurs at the very end of the Energistics model
                // update
                EA.TaggedValue tag = energisticsClass.TaggedValues.GetByName(Constants.fesapiNamespaceTagName);

                // creating the EpcDocument std::vector attribute for easily access the current Energistics class
                string attributeName = Char.ToLowerInvariant(energisticsClass.Name[0]) + energisticsClass.Name.Substring(1) + "Set";
                string attributeType = "std::vector<" + tag.Value + "::" + energisticsClass.Name + "*>";
                EA.Attribute attribute = attribute = energisticsEpcDocument.Attributes.AddNew(attributeName, attributeType);
                attribute.Visibility = "Private";
                if (!(attribute.Update()))
                {
                    Tool.showMessageBox(repository, attribute.GetLastError());
                    continue;
                }
                energisticsEpcDocument.Attributes.Refresh();

                // thanks to a tag, we tell to the transformation engine (transforming the Energistics model into a C++ model)
                // to preserve our easy access vector attribute in the C++ model
                EA.AttributeTag generationTag = attribute.TaggedValues.AddNew(Constants.fesapiGenerationTagName, "true");
                if (!(generationTag.Update()))
                {
                    Tool.showMessageBox(repository, generationTag.GetLastError());
                    continue;
                }
                attribute.TaggedValues.Refresh();

                // we want to generate by hand the corresponding getter. Thanks to a tag, we tell to the model
                // transformation engine (transforming the Energistics model into a C++ model) to do not automatically
                // generate a getter
                EA.AttributeTag getterGenerationTag = attribute.TaggedValues.AddNew(Constants.fesapiGetterGenerationTagName, "false");
                if (!(getterGenerationTag.Update()))
                {
                    Tool.showMessageBox(repository, getterGenerationTag.GetLastError());
                    continue;
                }

                // we create by hand the corresponding vector
                string methodName = "get" + energisticsClass.Name + "Set";
                string methodType = attributeType + " &";
                EA.Method method = energisticsEpcDocument.Methods.AddNew(methodName, methodType);
                method.IsConst = true;
                method.Stereotype = "const";
                if (!(method.Update()))
                {
                    Tool.showMessageBox(repository, method.GetLastError());
                    continue;
                }
                energisticsEpcDocument.Methods.Refresh();

                // associating the Energistics class to its fesapi namespace
                if (!(importNamespace.ContainsKey(tag.Value)))
                    importNamespace.Add(tag.Value, new List<string>());
                importNamespace[tag.Value].Add(energisticsClass.Name);

            }

            // converting the association between fesapi namespace and Energistics classes into a
            // EpcDocument class tag. This tag will be used during the code generation for copying 
            // forward declarations corresponding to easy access vectors into EpcDocument.h
            string importNameSpaceTagValue = "";
            foreach (KeyValuePair<string, List<string>> kv in importNamespace)
            {
                importNameSpaceTagValue += "namespace " + kv.Key + " {";
                List<string> classNameList = kv.Value;
                foreach (string className in classNameList)
                {
                    importNameSpaceTagValue += "class " + className + ";";
                }
                importNameSpaceTagValue += "}";
            }
            EA.TaggedValue importNamespaceTag = energisticsEpcDocument.TaggedValues.AddNew(Constants.fesapiImportNamespaceTag, importNameSpaceTagValue);
            if (!(importNamespaceTag.Update()))
                Tool.showMessageBox(repository, importNamespaceTag.GetLastError());
            energisticsEpcDocument.TaggedValues.Refresh();
        }

        #endregion
    }
}
