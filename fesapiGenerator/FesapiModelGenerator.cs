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
        /// Energistics common/v2.0 package
        /// </summary>
        private EA.Package energisticsCommon2_0Package;

        /// <summary>
        /// Energistics common/v2.2 package
        /// </summary>
        private EA.Package energisticsCommon2_2Package;

        /// <summary>
        /// Energistics resqml/V2.0.1 package
        /// </summary>
        private EA.Package energisticsResqml2_0_1Package;

        /// <summary>
        /// Energistics resqml/V2.2 package
        /// </summary>
        private EA.Package energisticsResqml2_2Package;

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

        /// <summary>
        /// Fesapi EpcDocument class
        /// </summary>
        private EA.Element epcDocumentClass;

        /// <summary>
        /// Fesapi AbstractObject class
        /// </summary>
        private EA.Element fesapiAbstractObjectClass;

        private List<EA.Element> fesapiResqml2ClassList = null;
        private List<EA.Element> fesapiResqml2_0_1ClassList = null;
        private List<EA.Element> fesapiResqml2_2ClassList = null;

        private Dictionary<EA.Element, EA.Element> fesapiResqml2ToEnergisticsResqml2_0_1 = null;
        private Dictionary<EA.Element, EA.Element> fesapiResqml2ToEnergisticsResqml2_2 = null;
        private Dictionary<EA.Element, EA.Element> fesapiResqml2_0_1toEnergisticsResqml2_0_1 = null;
        private Dictionary<EA.Element, EA.Element> fesapiResqml2_2toEnergisticsResqml2_2 = null;



        #endregion

        #region constructor

        public FesapiModelGenerator(
            EA.Repository repository,
            EA.Package energisticsCommon2_0Package, EA.Package energisticsCommon2_2Package, EA.Package energisticsResqml2_0_1Package, EA.Package energisticsResqml2_2Package,
            EA.Package fesapiCommonPackage, EA.Package fesapiResqml2Package, EA.Package fesapiResqml2_0_1Package, EA.Package fesapiResqml2_2Package)
        {
            this.repository = repository;
            this.energisticsCommon2_0Package = energisticsCommon2_0Package;
            this.energisticsCommon2_2Package = energisticsCommon2_2Package;
            this.energisticsResqml2_0_1Package = energisticsResqml2_0_1Package;
            this.energisticsResqml2_2Package = energisticsResqml2_2Package;
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

        public void generateFesapiModel()
        {
            // Adding EpcDocument and AbstractObject classes in the fesapi/common package
            epcDocumentClass = addFesapiClass("EpcDocument", fesapiCommonPackage);
            fesapiAbstractObjectClass = addFesapiClass("AbstractObject", fesapiCommonPackage);
            if ((epcDocumentClass == null) || (fesapiAbstractObjectClass == null))
            {
                Tool.showMessageBox(repository, "Unable to create EpcDocument class and/or AbstractObject class in fesapi/Class Model/common !");
                return;
            }
            fesapiCommonPackage.Elements.Refresh();

            // getting all Resqml 2.0.1 top level classes
            List<EA.Element> energisticsResqml2_0_1ClassList = new List<EA.Element>();
            Tool.fillElementList(energisticsResqml2_0_1Package, energisticsResqml2_0_1ClassList, "Class", true);
            Tool.fillElementList(energisticsCommon2_0Package, energisticsResqml2_0_1ClassList, "Class", true);

            // getting all Resqml 2.2 top level classes
            List<EA.Element> energisticsResqml2_2ClassList = new List<EA.Element>();
            Tool.fillElementList(energisticsResqml2_2Package, energisticsResqml2_2ClassList, "Class", true);
            Tool.fillElementList(energisticsCommon2_2Package, energisticsResqml2_2ClassList, "Class", true);

            // we look at all Resqml 2.0.1 classes
            foreach (EA.Element energisticsResqml2_0_1Class in energisticsResqml2_0_1ClassList)
            {
                string energisticsClassName = energisticsResqml2_0_1Class.Name;

                // ***************************************************************************************************************************************************************
                // ***************************************************************************************************************************************************************
                // DEBUG: uncomment to accelerate process by focusing on Local3dCrs classes
                if (!(energisticsClassName.Equals("AbstractLocal3dCrs")) && !(energisticsClassName.Equals("LocalDepth3dCrs")) && !(energisticsClassName.Equals("LocalTime3dCrs")) && !(energisticsClassName.Equals("Activity")))
                    continue;
                // ***************************************************************************************************************************************************************
                // ***************************************************************************************************************************************************************
                
                // does it exists such a class (with the same name) in Resqml 2.2?
                EA.Element energisticsResqml2_2Class = energisticsResqml2_2ClassList.Find(c => c.Name.Equals(energisticsClassName));
                if (energisticsResqml2_2Class != null)
                {
                    // if yes, such class must be added to fesapi/resqml2
                    EA.Element fesapiResqml2Class = addFesapiClass(energisticsClassName, fesapiResqml2Package);
                    if (fesapiResqml2Class == null)
                    {
                        Tool.log(repository, "Unable to add " + energisticsClassName + " class in fesapi/Class Model/resqml2 !");
                        continue;
                    }
                    fesapiResqml2ClassList.Add(fesapiResqml2Class);
                    fesapiResqml2ToEnergisticsResqml2_0_1.Add(fesapiResqml2Class, energisticsResqml2_0_1Class);
                    fesapiResqml2ToEnergisticsResqml2_2.Add(fesapiResqml2Class, energisticsResqml2_2Class);

                    //exploreResqml2RelationSet(energisticsResqml2_0_1Class, energisticsResqml2_2Class);

                    // if the class is a leaf, it must also be added to both fesapi/resqml2_0_1 and fesapi/resqml2_2 packages
                    if (Tool.isLeaf(energisticsResqml2_0_1Class))
                    {
                        EA.Element fesapiResqml2_0_1Class = addFesapiClass(energisticsClassName, fesapiResqml2_0_1Package);
                        if (fesapiResqml2_0_1Class == null)
                        {
                            Tool.log(repository, "Unable to add " + energisticsClassName + " class in fesapi/Class Model/resqml2_0_1!");
                            continue;
                        }
                        fesapiResqml2_0_1ClassList.Add(fesapiResqml2_0_1Class);
                        fesapiResqml2_0_1toEnergisticsResqml2_0_1.Add(fesapiResqml2_0_1Class, energisticsResqml2_0_1Class);


                        EA.Element fesapiResqml2_2Class = addFesapiClass(energisticsClassName, fesapiResqml2_2Package);
                        if (fesapiResqml2_2Class == null)
                        {
                            Tool.log(repository, "Unable to add " + energisticsClassName + " class in fesapi/Class Model/resqml2_2!");
                            continue;
                        }
                        fesapiResqml2_2ClassList.Add(fesapiResqml2_2Class);
                        fesapiResqml2_2toEnergisticsResqml2_2.Add(fesapiResqml2_2Class, energisticsResqml2_2Class);
                    }
                }
                else
                {
                    // if there is no such a class in resqml 2.2, then it must be added only into fesapi/resqml2_0_1
                    EA.Element fesapiResqml2_0_1Class = addFesapiClass(energisticsClassName, fesapiResqml2_0_1Package);
                    if (fesapiResqml2_0_1Class == null)
                    {
                        Tool.log(repository, "Unable to add " + energisticsClassName + " class in fesapi/Class Model/resqml2_0_1!");
                        continue;
                    }
                    fesapiResqml2_0_1ClassList.Add(fesapiResqml2_0_1Class);
                    fesapiResqml2_0_1toEnergisticsResqml2_0_1.Add(fesapiResqml2_0_1Class, energisticsResqml2_0_1Class);
                }
            }

            // we look at remaining resqml 2.2 classes (classes whose are not common with resqml 2.0.1)
            foreach (EA.Element energisticsResqml2_2Class in energisticsResqml2_2ClassList)
            {
                string energisticsClassName = energisticsResqml2_2Class.Name;

                // ***************************************************************************************************************************************************************
                // ***************************************************************************************************************************************************************
                // DEBUG: uncomment to accelerate process by focusing on Local3dCrs classes
                if (!(energisticsClassName.Equals("AbstractLocal3dCrs")) && !(energisticsClassName.Equals("LocalDepth3dCrs")) && !(energisticsClassName.Equals("LocalTime3dCrs")) && !(energisticsClassName.Equals("Activity")))
                    continue;
                // ***************************************************************************************************************************************************************
                // ***************************************************************************************************************************************************************

                // does it exists such a class in resqml 2.0.1
                EA.Element energisticsResqml2_0_1Class = energisticsResqml2_0_1ClassList.Find(c => c.Name.Equals(energisticsClassName));
                if (energisticsResqml2_0_1Class == null)
                {
                    //if no, it only belongs to fesapi/resqml2_2
                    EA.Element fesapiResqml2_2Class = addFesapiClass(energisticsClassName, fesapiResqml2_2Package);
                    if (fesapiResqml2_2Class == null)
                    {
                        Tool.log(repository, "Unable to add " + energisticsClassName + " class in fesapi/Class Model/resqml2_2!");
                        continue;
                    }
                    fesapiResqml2_2ClassList.Add(fesapiResqml2_2Class);
                    fesapiResqml2_2toEnergisticsResqml2_2.Add(fesapiResqml2_2Class, energisticsResqml2_2Class);
                }
            }

            fesapiResqml2Package.Elements.Refresh();
            fesapiResqml2_0_1Package.Elements.Refresh();
            fesapiResqml2_2Package.Elements.Refresh();

            generateInheritance();
            generateConstructorSet();
            generateXmlTagSet();
            //generateGetterSet();

            foreach (EA.Element fesapiResqml2Class in fesapiResqml2ClassList)
            {
                EA.Element energisticsResqml2_0_1Class = fesapiResqml2ToEnergisticsResqml2_0_1[fesapiResqml2Class];
                EA.Element energisticsResqml2_2Class = fesapiResqml2ToEnergisticsResqml2_2[fesapiResqml2Class];

                exploreResqml2RelationSet(fesapiResqml2Class, energisticsResqml2_0_1Class, energisticsResqml2_2Class);
            }
            foreach (EA.Element fesapiResqml2_0_1Class in fesapiResqml2_0_1ClassList)
            {
                EA.Element fesapiResqml2Class = fesapiResqml2ClassList.Find(c => c.Name.Equals(fesapiResqml2_0_1Class.Name));
                if (fesapiResqml2Class == null)
                {
                    EA.Element energisticsResqml2_0_1Class = fesapiResqml2ToEnergisticsResqml2_0_1[fesapiResqml2Class];
                    exploreRelationSet(fesapiResqml2_0_1Class, energisticsResqml2_0_1Class);
                }
            }
            foreach (EA.Element fesapiResqml2_2Class in fesapiResqml2_2ClassList)
            {
                EA.Element fesapiResqml2Class = fesapiResqml2ClassList.Find(c => c.Name.Equals(fesapiResqml2_2Class.Name));
                if (fesapiResqml2Class == null)
                {
                    EA.Element energisticsResqml2_2Class = fesapiResqml2ToEnergisticsResqml2_2[fesapiResqml2Class];
                    exploreRelationSet(fesapiResqml2_2Class, energisticsResqml2_2Class);
                }
            }

            // make sure the model view is up to date in the Enterprise Architect GUI
            repository.RefreshModelView(0);
        }

        #endregion

        #region private methods

        private EA.Element addFesapiClass(string className, EA.Package fesapiPackage)
        {
            EA.Element fesapiClass = fesapiPackage.Elements.AddNew(className, "Class");
            fesapiClass.Gentype = "C++ Fesapi 2";
            if (!(fesapiClass.Update()))
            {
                Tool.log(repository, fesapiClass.GetLastError());
                return null;
            }
            fesapiClass.Elements.Refresh();

            return fesapiClass;
        }

        #region inheritance

        private void generateInheritance()
        {
            // handling fesapi/resqml2 classes
            foreach (EA.Element fesapiResqml2Class in fesapiResqml2ClassList)
            {
                // getting Energistics Resqml 2.0.1 base class name
                EA.Element energisticsResqml2_0_1Class = fesapiResqml2ToEnergisticsResqml2_0_1[fesapiResqml2Class];
                if (energisticsResqml2_0_1Class.BaseClasses.Count != 1)
                {
                    Tool.log(repository, energisticsResqml2_0_1Class.Name + " class inherits from 0 are more than 1 class(es) in Resqml 2.0.1. No generalization connector will be generated!");
                    continue;
                }
                string energisticsResqml2_0_1BaseClassName = energisticsResqml2_0_1Class.BaseClasses.GetAt(0).Name;

                // getting Energistics Resqml 2.2 base class name
                EA.Element energisticsResqml2_2Class = fesapiResqml2ToEnergisticsResqml2_2[fesapiResqml2Class];
                if (energisticsResqml2_2Class.BaseClasses.Count != 1)
                {
                    Tool.log(repository, energisticsResqml2_2Class.Name + " class inherits from 0 are more than 1 class(es) in Resqml 2.2. No generalization connector will be generated!");
                    continue;
                }
                string energisticsResqml2_2BaseClassName = energisticsResqml2_2Class.BaseClasses.GetAt(0).Name;

                // first, we look for fesapi/resqml2 classes inheriting from fesapi/common/AbstractObject
                if (energisticsResqml2_0_1BaseClassName.Equals(Constants.energisticsResqml2_0_1AbstractObjectClassName) &&
                    energisticsResqml2_2BaseClassName.Equals(Constants.energisticsResqml2_2AbstractObjectClassName))
                {
                    if (addGeneralizationConnector(fesapiResqml2Class, fesapiAbstractObjectClass) == null)
                    {
                        Tool.log(repository, "Unable to properly add a generalization connector from fesapi/Class Model/resqml2/" + fesapiResqml2Class.Name + " to fesapi/Class Model/common/AbstractObject!");
                        continue;
                    }
                }
                else // then, we look for fesapi/resqml2 classes inheriting from fesapi/resqml2 classes
                {
                    if (!(energisticsResqml2_0_1BaseClassName.Equals(energisticsResqml2_2BaseClassName)))
                    {
                        Tool.log(repository, energisticsResqml2_2Class.Name + " inherits from different classes in Resqml 2.0.1 and Resqml 2.2. No generalization connector will be generated!");
                        continue;
                    }

                    EA.Element fesapiResqml2ParentClass = fesapiResqml2ClassList.Find(c => energisticsResqml2_2Class.BaseClasses.GetAt(0).Name.Equals(c.Name));
                    if (fesapiResqml2ParentClass == null)
                    {
                        Tool.log(repository, "Base class of fesapi/Class Model/resqml2/" + fesapiResqml2Class.Name + " does not exist in fesapi/Class Model/resqml2. No generalization connector will be generated!");
                        continue;
                    }
                    else
                    {
                        if (addGeneralizationConnector(fesapiResqml2Class, fesapiResqml2ParentClass) == null)
                        {
                            Tool.log(repository, "Unable to properly add a generalization connector from fesapi/Class Model/resqml2/" + fesapiResqml2Class.Name + " to fesapi/Class Model/resqml2/" + fesapiResqml2ParentClass.Name + "!");
                            continue;
                        }
                    }
                }
            }

            // handling fesapi/resqml2_0_1 classes
            foreach (EA.Element fesapiResqml2_0_1Class in fesapiResqml2_0_1ClassList)
            {
                EA.Element energisticsResqml2_0_1Class = fesapiResqml2_0_1toEnergisticsResqml2_0_1[fesapiResqml2_0_1Class];
                if (energisticsResqml2_0_1Class.BaseClasses.Count != 1)
                {
                    Tool.log(repository, energisticsResqml2_0_1Class.Name + " class inherits from 0 are more than 1 class(es) in Resqml 2.0.1. No generalization connector will be generated!");
                    continue;
                }
                string energisticsResqml2_0_1BaseClassName = energisticsResqml2_0_1Class.BaseClasses.GetAt(0).Name;

                // first, we look if the parent class is in fesapi/resqml2_0_1
                EA.Element fesapiResqml2_0_1ParentClass = fesapiResqml2_0_1ClassList.Find(c => energisticsResqml2_0_1BaseClassName.Equals(c.Name));
                if (fesapiResqml2_0_1ParentClass != null)
                {
                    if (addGeneralizationConnector(fesapiResqml2_0_1Class, fesapiResqml2_0_1ParentClass) == null)
                    {
                        Tool.log(repository, "Unable to properly add generalization connector from fesapi/Class Model/resqml2_0_1/" + fesapiResqml2_0_1Class.Name + " to fesapi/Class Model/resqml2_0_1/" + fesapiResqml2_0_1ParentClass.Name + "!");
                        continue;
                    }
                    continue;
                }

                // then, we look if there is a class with the same name in fesapi/resqml2
                EA.Element fesapiResqml2ParentClass = fesapiResqml2ClassList.Find(c => c.Name.Equals(fesapiResqml2_0_1Class.Name));
                if (fesapiResqml2ParentClass != null)
                {
                    if (addGeneralizationConnector(fesapiResqml2_0_1Class, fesapiResqml2ParentClass) == null)
                    {
                        Tool.log(repository, "Unable to properly add generalization connector from fesapi/Class Model/resqml2_0_1/" + fesapiResqml2_0_1Class.Name + " to fesapi/Class Model/resqml2/" + fesapiResqml2ParentClass.Name + "!");
                        continue;
                    }
                    continue;
                }

                // then, we look if the parent class is in fesapi/resqml2
                fesapiResqml2ParentClass = fesapiResqml2ClassList.Find(c => energisticsResqml2_0_1BaseClassName.Equals(c.Name));
                if (fesapiResqml2ParentClass != null)
                {
                    if (addGeneralizationConnector(fesapiResqml2_0_1Class, fesapiResqml2ParentClass) == null)
                    {
                        Tool.log(repository, "Unable to properly add generalization connector from fesapi/Class Model/resqml2_0_1/" + fesapiResqml2_0_1Class.Name + " to fesapi/Class Model/resqml2/" + fesapiResqml2ParentClass.Name + "!");
                        continue;
                    }
                    continue;
                }

                // finally, we look if the parent class is fesapi/common/AbstractObject
                if (energisticsResqml2_0_1BaseClassName.Equals(Constants.energisticsResqml2_0_1AbstractObjectClassName))
                {
                    if (addGeneralizationConnector(fesapiResqml2_0_1Class, fesapiAbstractObjectClass) == null)
                    {
                        Tool.log(repository, "Unable to properly add generalization connector from fesapi/Class Model/resqml2_0_1/" + fesapiResqml2_0_1Class.Name + " to fesapi/Class Model/common/AbstractObject!");
                        continue;
                    }
                    continue;
                }

                // here, we did not find the parent class in fesapi
                Tool.log(repository, "Base class of fesapi/Class Model/resqml2_0_1" + fesapiResqml2ParentClass.Name + " does not exist in fesapi/Class Model. No generalization connector will be generated!");
            }


            //foreach (EA.Element fesapiResqml2_0_1Class in fesapiResqml2_0_1ClassList)
            //{
            //    EA.Element energisticsResqml2_0_1Class = fesapiResqml2_0_1toEnergisticsResqml2_0_1[fesapiResqml2_0_1Class];
            //    if (energisticsResqml2_0_1Class.BaseClasses.Count != 1)
            //    {
            //        Tool.log(repository, energisticsResqml2_0_1Class.Name + " class inherits from 0 are more than 1 class(es) in Resqml 2.0.1. No generalization connector will be generated!");
            //        continue;
            //    }
            //    string energisticsResqml2_0_1BaseClassName = energisticsResqml2_0_1Class.BaseClasses.GetAt(0).Name;

            //    // first, we look for fesapi/resqml2_0_1 classes inheriting from fesapi/common/AbstractObject
            //    if (energisticsResqml2_0_1BaseClassName.Equals(Constants.energisticsResqml2_0_1AbstractObjectClassName))
            //    {
            //        if (addGeneralizationConnector(fesapiResqml2_0_1Class, fesapiAbstractObjectClass) == null)
            //        {
            //            Tool.log(repository, "Unable to properly add generalization connector from fesapi/Class Model/resqml2_0_1/" + fesapiResqml2_0_1Class.Name + " to fesapi/Class Model/common/AbstractObject!");
            //            continue;
            //        }
            //    }
            //    else
            //    {
            //        // then, we look if the parent class is in fesapi/resqml2_0_1 
            //        EA.Element fesapiResqml2_0_1ParentClass = fesapiResqml2_0_1ClassList.Find(c => energisticsResqml2_0_1BaseClassName.Equals(c.Name));
            //        if (fesapiResqml2_0_1ParentClass != null)
            //        {
            //            if (addGeneralizationConnector(fesapiResqml2_0_1Class, fesapiResqml2_0_1ParentClass) == null)
            //            {
            //                Tool.log(repository, "Unable to properly add generalization connector from fesapi/Class Model/resqml2_0_1/" + fesapiResqml2_0_1Class.Name + " to fesapi/Class Model/resqml2_0_1/" + fesapiResqml2_0_1ParentClass.Name + "!");
            //                continue;
            //            }
            //        }
            //        else
            //        {
            //            // then, we look if there is a class in fesapi/resqml2 with the same name
            //            EA.Element fesapiResqml2ParentClass = fesapiResqml2ClassList.Find(c => c.Name.Equals(fesapiResqml2_0_1Class.Name));
            //            if (fesapiResqml2ParentClass == null)
            //            {
            //                // finally, we look if the parent class is in fesapi/resqml2
            //                fesapiResqml2ParentClass = fesapiResqml2ClassList.Find(c => energisticsResqml2_0_1BaseClassName.Equals(c.Name));
            //            }

            //            if (fesapiResqml2ParentClass == null)
            //            {
            //                Tool.log(repository, "Base class of fesapi/Class Model/resqml2_0_1" + fesapiResqml2ParentClass.Name + " does not exist in fesapi/Class Model. No generalization connector will be generated!");
            //                continue;
            //            }
            //            else
            //            {
            //                if (addGeneralizationConnector(fesapiResqml2_0_1Class, fesapiResqml2ParentClass) == null)
            //                {
            //                    Tool.log(repository, "Unable to properly add generalization connector from fesapi/Class Model/resqml2_0_1/" + fesapiResqml2_0_1Class.Name + " to fesapi/Class Model/resqml2_0_1/" + fesapiResqml2ParentClass.Name + "!");
            //                    continue;
            //                }
            //            }
            //        }
            //    }
            //}

            // handling fesapi/resqml2_2 classes
            foreach (EA.Element fesapiResqml2_2Class in fesapiResqml2_2ClassList)
            {
                EA.Element energisticsResqml2_2Class = fesapiResqml2_2toEnergisticsResqml2_2[fesapiResqml2_2Class];
                if (energisticsResqml2_2Class.BaseClasses.Count != 1)
                {
                    Tool.log(repository, energisticsResqml2_2Class.Name + " class inherits from 0 are more than 1 class(es) in Resqml 2.2. No generalization connector will be generated!");
                    continue;
                }
                string energisticsResqml2_2BaseClassName = energisticsResqml2_2Class.BaseClasses.GetAt(0).Name;

                // first, we look if the parent class is in fesapi/resqml2_2
                EA.Element fesapiResqml2_2ParentClass = fesapiResqml2_2ClassList.Find(c => energisticsResqml2_2BaseClassName.Equals(c.Name));
                if (fesapiResqml2_2ParentClass != null)
                {
                    if (addGeneralizationConnector(fesapiResqml2_2Class, fesapiResqml2_2ParentClass) == null)
                    {
                        Tool.log(repository, "Unable to properly add generalization connector from fesapi/Class Model/resqml2_2/" + fesapiResqml2_2Class.Name + " to fesapi/Class Model/resqml2_2/" + fesapiResqml2_2ParentClass.Name + "!");
                        continue;
                    }
                    continue;
                }

                // then, we look if there is a class with the same name in fesapi/resqml2
                EA.Element fesapiResqml2ParentClass = fesapiResqml2ClassList.Find(c => c.Name.Equals(fesapiResqml2_2Class.Name));
                if (fesapiResqml2ParentClass != null)
                {
                    if (addGeneralizationConnector(fesapiResqml2_2Class, fesapiResqml2ParentClass) == null)
                    {
                        Tool.log(repository, "Unable to properly add generalization connector from fesapi/Class Model/resqml2_2/" + fesapiResqml2_2Class.Name + " to fesapi/Class Model/resqml2/" + fesapiResqml2ParentClass.Name + "!");
                        continue;
                    }
                    continue;
                }

                // then, we look if the parent class is in fesapi/resqml2
                fesapiResqml2ParentClass = fesapiResqml2ClassList.Find(c => energisticsResqml2_2BaseClassName.Equals(c.Name));
                if (fesapiResqml2ParentClass != null)
                {
                    if (addGeneralizationConnector(fesapiResqml2_2Class, fesapiResqml2ParentClass) == null)
                    {
                        Tool.log(repository, "Unable to properly add generalization connector from fesapi/Class Model/resqml2_2/" + fesapiResqml2_2Class.Name + " to fesapi/Class Model/resqml2/" + fesapiResqml2ParentClass.Name + "!");
                        continue;
                    }
                    continue;
                }

                // finally, we look if the parent class is fesapi/common/AbstractObject
                if (energisticsResqml2_2BaseClassName.Equals(Constants.energisticsResqml2_2AbstractObjectClassName))
                {
                    if (addGeneralizationConnector(fesapiResqml2_2Class, fesapiAbstractObjectClass) == null)
                    {
                        Tool.log(repository, "Unable to properly add generalization connector from fesapi/Class Model/resqml2_2/" + fesapiResqml2_2Class.Name + " to fesapi/Class Model/common/AbstractObject!");
                        continue;
                    }
                    continue;
                }

                // here, we did not find the parent class in fesapi
                Tool.log(repository, "Base class of fesapi/Class Model/resqml2_2" + fesapiResqml2ParentClass.Name + " does not exist in fesapi/Class Model. No generalization connector will be generated!");
            }

            //// handling fesapi/resqml2_2 classes
            //foreach (EA.Element fesapiResqml2_2Class in fesapiResqml2_2ClassList)
            //{
            //    EA.Element energisticsResqml2_2Class = fesapiResqml2_2toEnergisticsResqml2_2[fesapiResqml2_2Class];
            //    if (energisticsResqml2_2Class.BaseClasses.Count != 1)
            //    {
            //        Tool.log(repository, energisticsResqml2_2Class.Name + " class inherits from 0 are more than 1 class(es) in Resqml 2.2. No generalization connector will be generated!");
            //        continue;
            //    }
            //    string energisticsResqml2_2BaseClassName = energisticsResqml2_2Class.BaseClasses.GetAt(0).Name;

            //    // first, we look for fesapi/resqml2_2 classes inheriting from  fesapi/common/AbstractObject
            //    if (energisticsResqml2_2Class.BaseClasses.GetAt(0).Name.Equals(Constants.energisticsResqml2_2AbstractObjectClassName))
            //    {
            //        if (addGeneralizationConnector(fesapiResqml2_2Class, fesapiAbstractObjectClass) == null)
            //        {
            //            Tool.log(repository, "Unable to properly add generalization connector from fesapi/Class Model/resqml2_2/" + fesapiResqml2_2Class.Name + " to fesapi/Class Model/common/AbstractObject!");
            //            continue;
            //        }
            //    }
            //    else
            //    {
            //        // then, we look if the parent class is in fesapi/resqml2_2 
            //        EA.Element fesapiResqml2_2ParentClass = fesapiResqml2_2ClassList.Find(c => energisticsResqml2_2BaseClassName.Equals(c.Name));
            //        if (fesapiResqml2_2ParentClass != null)
            //        {
            //            if (addGeneralizationConnector(fesapiResqml2_2Class, fesapiResqml2_2ParentClass) == null)
            //            {
            //                Tool.log(repository, "Unable to properly add generalization connector from fesapi/Class Model/resqml2_2/" + fesapiResqml2_2Class.Name + " to fesapi/Class Model/resqml2_2/" + fesapiResqml2_2ParentClass.Name + "!");
            //                continue;
            //            }
            //        }
            //        else
            //        {
            //            // then, we look if there is a class in fesapi/resqml2 with the same name
            //            EA.Element fesapiResqml2ParentClass = fesapiResqml2ClassList.Find(c => c.Name.Equals(fesapiResqml2_2Class.Name));
            //            if (fesapiResqml2ParentClass == null)
            //            {
            //                // finally, we look if the parent class is in fesapi/resqml2
            //                fesapiResqml2ParentClass = fesapiResqml2ClassList.Find(c => energisticsResqml2_2BaseClassName.Equals(c.Name));
            //            }

            //            if (fesapiResqml2ParentClass == null)
            //            {
            //                Tool.log(repository, "Base class of fesapi/Class Model/resqml2_2" + fesapiResqml2ParentClass.Name + " does not exist in fesapi/Class Model. No generalization connector will be generated!");
            //                continue;
            //            }
            //            else
            //            {
            //                if (addGeneralizationConnector(fesapiResqml2_2Class, fesapiResqml2ParentClass) == null)
            //                {
            //                    Tool.log(repository, "Unable to properly add generalization connector from fesapi/Class Model/resqml2_2/" + fesapiResqml2_2Class.Name + " to fesapi/Class Model/resqml2_0_1/" + fesapiResqml2ParentClass.Name + "!");
            //                    continue;
            //                }
            //            }
            //        }

            //    }
            //}
        }

        private EA.Connector addGeneralizationConnector(EA.Element childClass, EA.Element baseClass)
        {
            EA.Connector generalizationConnector = childClass.Connectors.AddNew("", "Generalization");
            generalizationConnector.SupplierID = baseClass.ElementID;
            if (!(generalizationConnector.Update()))
            {
                Tool.log(repository, generalizationConnector.GetLastError());
                return null;
            }
            childClass.Connectors.Refresh();

            // we set up custom tag for copying the #include directive in the generated code
            string baseClassPackageName = repository.GetPackageByID(baseClass.PackageID).Name;
            string childclassPackageName = repository.GetPackageByID(childClass.PackageID).Name;
            string includeTagValue = "";
            includeTagValue += baseClassPackageName + "/";
            includeTagValue += baseClass.Name + ".h";
            EA.TaggedValue fesapiIncludeTag = childClass.TaggedValues.AddNew(Constants.fesapiBaseClassIncludeTagName, includeTagValue);
            if (!(fesapiIncludeTag.Update()))
            {
                Tool.log(repository, fesapiIncludeTag.GetLastError());
                return null;
            }

            // tagging the child class with the fesapi base class namespace (that is to say the name of its parent package)
            EA.TaggedValue fesapiBaseClassNamespaceTag = childClass.TaggedValues.AddNew("fesapiBaseClassNamespace", baseClassPackageName);
            if (!(fesapiBaseClassNamespaceTag.Update()))
            {
                Tool.log(repository, fesapiBaseClassNamespaceTag.GetLastError());
                return null;
            }

            childClass.TaggedValues.Refresh();

            return generalizationConnector;
        }

        #endregion

        #region constructor/destructor

        private void generateConstructorSet()
        {
            // handling fesapi/resqml2 classes
            foreach (EA.Element resqml2Class in fesapiResqml2ClassList)
            {
                if (addDefaultConstructor(resqml2Class, "protected") == null)
                {
                    Tool.log(repository, "Unable to properly add a default constructor in fesapi/Class Model/resqml2/" + resqml2Class.Name + "!");
                }
                if (addPartialTransfertConstructor(resqml2Class, "protected", Constants.gsoapEqml2_0Prefix) == null)
                {
                    Tool.log(repository, "Unable to properly add a partial transfert constructor in fesapi/Class Model/resqml2/" + resqml2Class.Name + "!");
                }
                if (addPartialTransfertConstructor(resqml2Class, "protected", Constants.gsoapEqml2_2Prefix) == null)
                {
                    Tool.log(repository, "Unable to properly add a partial transfert constructor in fesapi/Class Model/resqml2/" + resqml2Class.Name + "!");
                }
                if (addDeserializationConstructor(resqml2Class, fesapiResqml2ToEnergisticsResqml2_0_1[resqml2Class], "protected") == null)
                {
                    Tool.log(repository, "Unable to properly add a deserialization constructor in fesapi/Class Model/resqml2/" + resqml2Class.Name + "!");
                }
                if (addDeserializationConstructor(resqml2Class, fesapiResqml2ToEnergisticsResqml2_2[resqml2Class], "protected") == null)
                {
                    Tool.log(repository, "Unable to properly add a deserialization constructor in fesapi/Class Model/resqml2/" + resqml2Class.Name + "!");
                }
                if (addDefaultDestructor(resqml2Class) == null)
                {
                    Tool.log(repository, "Unable to properly add a default destructor in fesapi/Class Model/resqml2/" + resqml2Class.Name + "!");
                }
            }

            // handling fesapi/resqml2_0_1 classes
            foreach (EA.Element resqml2_0_1Class in fesapiResqml2_0_1ClassList)
            {
                if (addDefaultConstructor(resqml2_0_1Class, "protected") == null)
                {
                    Tool.log(repository, "Unable to properly add a default constructor in fesapi/Class Model/resqml2_0_1/" + resqml2_0_1Class.Name + "!");
                }
                string constructorVisibility;
                if (Tool.isAbstract(resqml2_0_1Class))
                {
                    constructorVisibility = "protected";
                }
                else
                {
                    constructorVisibility = "public";
                }
                if (addPartialTransfertConstructor(resqml2_0_1Class, constructorVisibility, Constants.gsoapEqml2_0Prefix) == null)
                {
                    Tool.log(repository, "Unable to properly add a partial transfert constructor in fesapi/Class Model/resqml2_0_1/" + resqml2_0_1Class.Name + "!");
                }
                if (addDeserializationConstructor(resqml2_0_1Class, fesapiResqml2_0_1toEnergisticsResqml2_0_1[resqml2_0_1Class], constructorVisibility) == null)
                {
                    Tool.log(repository, "Unable to properly add a deserialization constructor in fesapi/Class Model/resqml2_0_1/" + resqml2_0_1Class.Name + "!");
                }
                if (addDefaultDestructor(resqml2_0_1Class) == null)
                {
                    Tool.log(repository, "Unable to properly add a default destructor in fesapi/Class Model/resqml2_0_1/" + resqml2_0_1Class.Name + "!");
                }
            }

            // handling fesapi/resqml2_2 classes
            foreach (EA.Element resqml2_2Class in fesapiResqml2_2ClassList)
            {
                if (addDefaultConstructor(resqml2_2Class, "protected") == null)
                {
                    Tool.log(repository, "Unable to properly add a default constructor in fesapi/Class Model/resqml2_2/" + resqml2_2Class.Name + "!");
                }
                string constructorVisibility;
                if (Tool.isAbstract(resqml2_2Class))
                {
                    constructorVisibility = "protected";
                }
                else
                {
                    constructorVisibility = "public";
                }
                if (addPartialTransfertConstructor(resqml2_2Class, constructorVisibility, Constants.gsoapEqml2_2Prefix) == null)
                {
                    Tool.log(repository, "Unable to properly add a partial transfert constructor in fesapi/Class Model/resqml2_2/" + resqml2_2Class.Name + "!");
                }
                if (addDeserializationConstructor(resqml2_2Class, fesapiResqml2_2toEnergisticsResqml2_2[resqml2_2Class], constructorVisibility) == null)
                {
                    Tool.log(repository, "Unable to properly add a deserialization constructor in fesapi/Class Model/resqml2_2/" + resqml2_2Class.Name + "!");
                }
                if (addDefaultDestructor(resqml2_2Class) == null)
                {
                    Tool.log(repository, "Unable to properly add a default destructor in fesapi/Class Model/resqml2_2/" + resqml2_2Class.Name + "!");
                }
            }
        }

        private EA.Method addDefaultConstructor(EA.Element fesapiClass, string visibility)
        {
            EA.Method constructor = fesapiClass.Methods.AddNew(fesapiClass.Name, "");
            constructor.Notes = "Default constructor\nSet the gsoap proxy to nullptr.";
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
                    return null;
                }
                constructor.TaggedValues.Refresh();
            }

            return constructor;
        }

        private EA.Method addDeserializationConstructor(EA.Element fesapiClass, EA.Element energisticsClass, string visibility)
        {
            EA.Method constructor = fesapiClass.Methods.AddNew(fesapiClass.Name, "");
            constructor.Code = "";
            constructor.Notes = "Creates an instance of this class by wrapping a gsoap instance.";
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

            // adding the deserialization parameter
            EA.Parameter parameter = constructor.Parameters.AddNew("fromGsoap", Tool.getGsoapName(repository, energisticsClass) + "* ");
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
                initializerTagValue += baseClass.Name + "(fromGsoap)";
                EA.MethodTag initializerTag = constructor.TaggedValues.AddNew("initializer", initializerTagValue);
                if (!(initializerTag.Update()))
                {
                    Tool.showMessageBox(repository, initializerTag.GetLastError());
                    return null;
                }
            }

            return constructor;
        }

        #endregion

        #region XML_TAG

        private void generateXmlTagSet()
        {
            // handling fesapi/resqml2 classes
            foreach (EA.Element resqml2Class in fesapiResqml2ClassList)
            {
                if (!(resqml2Class.Name.StartsWith("Abstract")))
                {
                    if (!addXmlTagAttributeWithGetter(resqml2Class))
                    {
                        Tool.log(repository, "Unable to properly add XML_TAG attribute and/or getter in fesapi/Class Model/resqml2/" + resqml2Class.Name + "!");
                    }
                }
            }

            // handling fesapi/resqml2_0_1 classes
            foreach (EA.Element resqml2_0_1Class in fesapiResqml2_0_1ClassList)
            {
                if ((fesapiResqml2ClassList.Find(c => c.Name.Equals(resqml2_0_1Class.Name))) == null)
                {
                    if (!addXmlTagAttributeWithGetter(resqml2_0_1Class))
                    {
                        Tool.log(repository, "Unable to properly add XML_TAG attribute and/or getter in fesapi/Class Model/resqml2_0_1/" + resqml2_0_1Class.Name + "!");
                    }
                }
            }

            // handling fesapi/resqml2_2 classes
            foreach (EA.Element resqml2_2Class in fesapiResqml2_2ClassList)
            {
                if ((fesapiResqml2ClassList.Find(c => c.Name.Equals(resqml2_2Class.Name))) == null)
                {
                    if (!addXmlTagAttributeWithGetter(resqml2_2Class))
                    {
                        Tool.log(repository, "Unable to properly add XML_TAG attribute and/or getter in fesapi/Class Model/resqml2_2/" + resqml2_2Class.Name + "!");
                    }
                }
            }
        }

        /// <summary>
        /// Add an XML_TAG attribute with its getter to a given fesapi class. 
        /// </summary>
        /// <param name="energisticsClass">A fesapi class</param>
        private bool addXmlTagAttributeWithGetter(EA.Element fesapiClass)
        {
            // we add an XML_TAG attribute
            EA.Attribute xmlTagAttribute = fesapiClass.Attributes.AddNew("XML_TAG", "char*");
            xmlTagAttribute.IsStatic = true;
            xmlTagAttribute.IsConst = true;
            xmlTagAttribute.Default = "\"" + fesapiClass.Name + "\"";
            if (!(xmlTagAttribute.Update()))
            {
                Tool.showMessageBox(repository, xmlTagAttribute.GetLastError());
                return false;
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
                return false;
            }
            fesapiClass.Methods.Refresh();

            // tag the getter in order for its body to be generated into the class declaration
            EA.MethodTag getXmlTagMethodBodyLocationTag = getXmlTagMethod.TaggedValues.AddNew("bodyLocation", "classDec");
            if (!(getXmlTagMethodBodyLocationTag.Update()))
            {
                Tool.showMessageBox(repository, getXmlTagMethodBodyLocationTag.GetLastError());
                return false;
            }
            getXmlTagMethod.TaggedValues.Refresh();

            return true;
        }

        #endregion

        #region relations

        private void exploreResqml2RelationSet(EA.Element fesapiClass, EA.Element energisticsResqml2_0_1Class, EA.Element energisticsResqml2_2Class)
        {
            Tool.log(repository, "========================");
            Tool.log(repository, "begin exploreResqml2RelationSet...");

            Tool.log(repository, "class: " + energisticsResqml2_0_1Class.Name);
            exploreResqml2RelationSetRec(
                fesapiClass,
                energisticsResqml2_0_1Class, energisticsResqml2_2Class,
                energisticsResqml2_0_1Class, energisticsResqml2_2Class,
                "",
                0, 0,
                "get",
                "static_cast<" +Tool.getGsoapName(repository, energisticsResqml2_0_1Class) + "*>(" + Tool.getGsoapProxyName(repository, energisticsResqml2_0_1Class) + ")",
                "static_cast<" + Tool.getGsoapName(repository, energisticsResqml2_2Class) + "*>(" + Tool.getGsoapProxyName(repository, energisticsResqml2_2Class) + ")");

            Tool.log(repository, "... end exploreResqml2RelationSet");
            Tool.log(repository, "========================");
        }

        private void exploreResqml2RelationSetRec(
            EA.Element fesapiClass,
            EA.Element startingEnergisticsResqml2_0_1Class, EA.Element startingEnergisticsResqml2_2Class, 
            EA.Element energisticsResqml2_0_1Class, EA.Element energisticsResqml2_2Class, 
            string markedGeneralizationSet, 
            uint resqml2_0_1Index, uint resqml2_2Index, 
            string getterName,
            string resqml2_0_1Path, string resqml2_2Path)
        {
            //if (resqml2_0_1Path != "gsoapProxy2_0_1" && resqml2_2Path != "gsoapProxy2_2")
            //if (energisticsResqml2_0_1Class != startingEnergisticsResqml2_0_1Class && energisticsResqml2_2Class != startingEnergisticsResqml2_2Class)
            if (resqml2_0_1Path != "static_cast<" + Tool.getGsoapName(repository, startingEnergisticsResqml2_0_1Class) + "*>(" + Tool.getGsoapProxyName(repository, startingEnergisticsResqml2_0_1Class) + ")" && resqml2_2Path != "static_cast<" + Tool.getGsoapName(repository, startingEnergisticsResqml2_2Class) + "*>(" + Tool.getGsoapProxyName(repository, startingEnergisticsResqml2_2Class) + ")")
            {
                if ((Tool.isTopLevelClass(energisticsResqml2_0_1Class) || energisticsResqml2_0_1Class.Name == "AbstractResqmlDataObject") && (Tool.isTopLevelClass(energisticsResqml2_0_1Class) || energisticsResqml2_0_1Class.Name == "AbstractObject"))
                {
                    Tool.log(repository, "TopLevelObject relation (resqml2): ");
                    Tool.log(repository, "- Resqml 2.0.1 code: " + resqml2_0_1Path);
                    Tool.log(repository, "- Resqml 2.2 code: " + resqml2_2Path);
                    return;
                }
                else if ((Tool.isTopLevelClass(energisticsResqml2_0_1Class) || energisticsResqml2_0_1Class.Name == "AbstractResqmlDataObject") || (Tool.isTopLevelClass(energisticsResqml2_0_1Class) || energisticsResqml2_0_1Class.Name == "AbstractObject"))
                {
                    exploreRelationSetRec(fesapiClass, startingEnergisticsResqml2_0_1Class, energisticsResqml2_0_1Class, markedGeneralizationSet, resqml2_0_1Index, getterName, resqml2_0_1Path);
                    exploreRelationSetRec(fesapiClass, startingEnergisticsResqml2_2Class, energisticsResqml2_2Class, markedGeneralizationSet, resqml2_2Index, getterName, resqml2_2Path);
                }
            }

            // on regarde tous les attributs côté Resqml 2.0.1
            foreach (EA.Attribute energisticsResqml2_0_1Attribute in energisticsResqml2_0_1Class.Attributes)
            {
                EA.Attribute energisticsResqml2_2Attribute = null;
                foreach (EA.Attribute currentEnergisticsResqml2_2Attribute in energisticsResqml2_2Class.Attributes)
                {
                    if (energisticsResqml2_0_1Attribute.Name == currentEnergisticsResqml2_2Attribute.Name)
                    {
                        energisticsResqml2_2Attribute = currentEnergisticsResqml2_2Attribute;
                        break;
                    }
                }

                // *******************************
                // on traite le code version 2.0.1

                string newResqml2_0_1Path;
                // si la multiplicité est supérieur à 1
                if (!(energisticsResqml2_0_1Attribute.UpperBound == "1"))
                {
                    newResqml2_0_1Path = resqml2_0_1Path + "->" + energisticsResqml2_0_1Attribute.Name + "[index" + resqml2_0_1Index + "]";
                }
                // sinon si l'attribut est optionnel
                else if (energisticsResqml2_0_1Attribute.LowerBound == "0")
                {
                    newResqml2_0_1Path = resqml2_0_1Path + "->(*" + energisticsResqml2_0_1Attribute.Name + ")";
                }
                // sinon s'il est unique et mandatory
                else
                {
                    newResqml2_0_1Path = resqml2_0_1Path + "->" + energisticsResqml2_0_1Attribute.Name;
                }

                if (energisticsResqml2_2Attribute != null)
                {
                    // *******************************
                    // on traite le code version 2.2

                    string newResqml2_2Path;
                    // si la multiplicité est supérieur à 1
                    if (!(energisticsResqml2_2Attribute.UpperBound == "1"))
                    {
                        newResqml2_2Path = resqml2_2Path + "->" + energisticsResqml2_2Attribute.Name + "[index" + resqml2_2Index + "]";
                    }
                    // sinon si l'attribut est optionnel
                    else if (energisticsResqml2_2Attribute.LowerBound == "0")
                    {
                        newResqml2_2Path = resqml2_2Path + "->(*" + energisticsResqml2_2Attribute.Name + ")";
                    }
                    // sinon s'il est unique et mandatory
                    else
                    {
                        newResqml2_2Path = resqml2_2Path + "->" + energisticsResqml2_2Attribute.Name;
                    }

                    Tool.log(repository, "Attribute relation (resqml2):");
                    Tool.log(repository, "- Resqml 2.0.1 code: " + newResqml2_0_1Path);
                    Tool.log(repository, "- Resqml 2.2 code: " + newResqml2_2Path);

                    addResqml2GetterSet(fesapiClass, energisticsResqml2_0_1Attribute, energisticsResqml2_2Attribute, getterName + energisticsResqml2_0_1Attribute.Name, newResqml2_0_1Path, newResqml2_2Path);
                }
                else
                {
                    Tool.log(repository, "Attribute relation (resqml2_0_1): " + newResqml2_0_1Path);

                    EA.Element fesapiResqml2_0_1Class = fesapiResqml2_0_1ClassList.Find(c => c.Name.Equals(fesapiClass.Name));
                    if (fesapiResqml2_0_1Class != null)
                    {
                        addResqmlGetterSet(fesapiResqml2_0_1Class, energisticsResqml2_0_1Attribute, getterName + energisticsResqml2_0_1Attribute.Name, newResqml2_0_1Path);
                    }
                    else
                    {
                        addResqmlGetterSet(fesapiClass, energisticsResqml2_0_1Attribute, getterName + energisticsResqml2_0_1Attribute.Name, newResqml2_0_1Path);
                    }
                }
            }

            // on explore les attributs côté Resqml 2.2 (pour chopper ceux qui ne sont pas communs)
            foreach (EA.Attribute energisticsResqml2_2Attribute in energisticsResqml2_2Class.Attributes)
            {
                EA.Attribute energisticsResqml2_0_1Attribute = null;
                foreach (EA.Attribute currentEnergisticsResqml2_0_1Attribute in energisticsResqml2_0_1Class.Attributes)
                {
                    if (energisticsResqml2_2Attribute.Name == currentEnergisticsResqml2_0_1Attribute.Name)
                    {
                        energisticsResqml2_0_1Attribute = currentEnergisticsResqml2_0_1Attribute;
                        break;
                    }
                }

                string newResqml2_2Path;
                if (energisticsResqml2_0_1Attribute == null)
                {
                    // si la multiplicité est supérieur à 1
                    if (!(energisticsResqml2_2Attribute.UpperBound == "1"))
                    {
                        newResqml2_2Path = resqml2_2Path + "->" + energisticsResqml2_2Attribute.Name + "[index" + resqml2_2Index + "]";
                    }
                    // sinon si l'attribut est optionnel
                    else if (energisticsResqml2_2Attribute.LowerBound == "0")
                    {
                        newResqml2_2Path = resqml2_2Path + "->(*" + energisticsResqml2_2Attribute.Name + ")";
                    }
                    // sinon s'il est unique et mandatory
                    else
                    {
                        newResqml2_2Path = resqml2_2Path + "->" + energisticsResqml2_2Attribute.Name;
                    }

                    Tool.log(repository, "Attribute relation (resqml2_2): " + newResqml2_2Path);

                    EA.Element fesapiResqml2_2Class = fesapiResqml2_2ClassList.Find(c => c.Name.Equals(fesapiClass.Name));
                    if (fesapiResqml2_2Class != null)
                    {
                        addResqmlGetterSet(fesapiResqml2_2Class, energisticsResqml2_2Attribute, getterName + energisticsResqml2_2Attribute.Name, newResqml2_2Path);
                    }
                    else
                    {
                        addResqmlGetterSet(fesapiClass, energisticsResqml2_2Attribute, getterName + energisticsResqml2_2Attribute.Name, newResqml2_2Path);
                    }
                }
            }

            // on regarde tous les connecteurs côté Resqml 2.0.1
            foreach (EA.Connector energisticsResqml2_0_1Connector in energisticsResqml2_0_1Class.Connectors)
            {
                // on traite d'abord le cas des connecteurs hors generalisation
                if (energisticsResqml2_0_1Connector.Type != "Generalization" && energisticsResqml2_0_1Connector.ClientID == energisticsResqml2_0_1Class.ElementID)
                {
                    EA.Connector energisticsResqml2_2Connector = null;
                    foreach (EA.Connector currentEnergisticsResqml2_2Connector in energisticsResqml2_2Class.Connectors)
                    {
                        if (currentEnergisticsResqml2_2Connector.Type != "Generalization" &&
                            currentEnergisticsResqml2_2Connector.ClientID == energisticsResqml2_2Class.ElementID &&
                            currentEnergisticsResqml2_2Connector.SupplierEnd.Role == energisticsResqml2_0_1Connector.SupplierEnd.Role)
                        {
                            energisticsResqml2_2Connector = currentEnergisticsResqml2_2Connector;
                            break;
                        }
                    }

                    // **********
                    // code 2.0.1

                    string newResqml2_0_1Path;
                    uint newResqml2_0_1Index = resqml2_0_1Index;
                    bool isResqml2_0_1MultipleCardinality = false;
                    if (energisticsResqml2_0_1Connector.SupplierEnd.Cardinality == "*" || energisticsResqml2_0_1Connector.SupplierEnd.Cardinality == "0..*" || energisticsResqml2_0_1Connector.SupplierEnd.Cardinality == "1..*")
                    {
                        newResqml2_0_1Path = resqml2_0_1Path + "->" + energisticsResqml2_0_1Connector.SupplierEnd.Role + "[index" + resqml2_0_1Index + "]";
                        newResqml2_0_1Index++;
                        isResqml2_0_1MultipleCardinality = true;
                    }
                    else if (energisticsResqml2_0_1Connector.SupplierEnd.Cardinality == "0..1")
                    {
                        // TODO: à tester !
                        newResqml2_0_1Path = resqml2_0_1Path + "->{" + energisticsResqml2_0_1Connector.SupplierEnd.Role + "}";
                    }
                    else
                    {
                        newResqml2_0_1Path = resqml2_0_1Path + "->" + energisticsResqml2_0_1Connector.SupplierEnd.Role;
                    }

                    if (energisticsResqml2_2Connector != null)
                    {
                        // ********
                        // code 2.2

                        string newResqml2_2Path;
                        uint newResqml2_2Index = resqml2_2Index;
                        bool isResqml2_2MultipleCardinality = false;
                        if (energisticsResqml2_2Connector.SupplierEnd.Cardinality == "*" || energisticsResqml2_2Connector.SupplierEnd.Cardinality == "0..*" || energisticsResqml2_2Connector.SupplierEnd.Cardinality == "1..*")
                        {
                            newResqml2_2Path = resqml2_2Path + "->" + energisticsResqml2_2Connector.SupplierEnd.Role + "[index" + resqml2_2Index + "]";
                            newResqml2_2Index++;
                            isResqml2_2MultipleCardinality = true;
                        }
                        else if (energisticsResqml2_2Connector.SupplierEnd.Cardinality == "0..1")
                        {
                            // TODO: à tester !
                            newResqml2_2Path = resqml2_2Path + "->{" + energisticsResqml2_2Connector.SupplierEnd.Role + "}";
                        }
                        else
                        {
                            newResqml2_2Path = resqml2_2Path + "->" + energisticsResqml2_2Connector.SupplierEnd.Role;
                        }

                        exploreResqml2RelationSetRec(
                            fesapiClass,
                            startingEnergisticsResqml2_0_1Class, startingEnergisticsResqml2_2Class,
                            repository.GetElementByID(energisticsResqml2_0_1Connector.SupplierID), repository.GetElementByID(energisticsResqml2_2Connector.SupplierID),
                            markedGeneralizationSet,
                            newResqml2_0_1Index, newResqml2_2Index,
                            getterName + energisticsResqml2_0_1Connector.SupplierEnd.Role,
                            newResqml2_0_1Path, newResqml2_2Path);

                        if (isResqml2_0_1MultipleCardinality && isResqml2_2MultipleCardinality)
                        {
                            if (addResqml2Getter(fesapiClass, getterName + energisticsResqml2_0_1Connector.SupplierEnd.Role + "Count", "unsigned int",
                                resqml2_0_1Path + "->" + energisticsResqml2_0_1Connector.SupplierEnd.Role,
                                resqml2_2Path + "->" + energisticsResqml2_2Connector.SupplierEnd.Role,
                                "res = " + resqml2_0_1Path + "->" + energisticsResqml2_0_1Connector.SupplierEnd.Role + ".size()",
                                "res = " + resqml2_2Path + "->" + energisticsResqml2_2Connector.SupplierEnd.Role + ".size()") == null)
                            {
                                Tool.log(repository, "Unable to properly add count getter for the " + energisticsResqml2_0_1Connector.SupplierEnd.Role + " relation of the fesapi/Class Model/" + repository.GetPackageByID(fesapiClass.PackageID).Name + "/" + fesapiClass.Name + "!");
                            }
                        }
                        else if (isResqml2_0_1MultipleCardinality)
                        {
                            if (addResqml2Getter(fesapiClass, getterName + energisticsResqml2_0_1Connector.SupplierEnd.Role + "Count", "unsigned int",
                                resqml2_0_1Path + "->" + energisticsResqml2_0_1Connector.SupplierEnd.Role,
                                "",
                                "res = " + resqml2_0_1Path + "->" + energisticsResqml2_0_1Connector.SupplierEnd.Role + ".size()",
                                "res = 1") == null)
                            {
                                Tool.log(repository, "Unable to properly add count getter for the " + energisticsResqml2_0_1Connector.SupplierEnd.Role + " relation of the fesapi/Class Model/" + repository.GetPackageByID(fesapiClass.PackageID).Name + "/" + fesapiClass.Name + "!");
                            }
                        }
                        else if (isResqml2_2MultipleCardinality)
                        {
                            if (addResqml2Getter(fesapiClass, getterName + energisticsResqml2_0_1Connector.SupplierEnd.Role + "Count", "unsigned int",
                                "",
                                resqml2_2Path + "->" + energisticsResqml2_2Connector.SupplierEnd.Role,
                                "res = 1",
                                "res = " + resqml2_2Path + "->" + energisticsResqml2_2Connector.SupplierEnd.Role + ".size()") == null)
                            {
                                Tool.log(repository, "Unable to properly add count getter for the " + energisticsResqml2_0_1Connector.SupplierEnd.Role + " relation of the fesapi/Class Model/" + repository.GetPackageByID(fesapiClass.PackageID).Name + "/" + fesapiClass.Name + "!");
                            }
                        }
                    }
                    else
                    {
                        EA.Element fesapiResqml2_0_1Class = fesapiResqml2_0_1ClassList.Find(c => c.Name.Equals(fesapiClass.Name));
                        if (fesapiResqml2_0_1Class == null)
                        {
                            fesapiResqml2_0_1Class = fesapiClass;
                        }

                        exploreRelationSetRec(
                            fesapiResqml2_0_1Class,
                            startingEnergisticsResqml2_0_1Class,
                            repository.GetElementByID(energisticsResqml2_0_1Connector.SupplierID), 
                            markedGeneralizationSet, 
                            newResqml2_0_1Index, 
                            getterName + energisticsResqml2_0_1Connector.SupplierEnd.Role,
                            newResqml2_0_1Path);

                        if (isResqml2_0_1MultipleCardinality)
                        {
                            if (addResqmlGetter(fesapiClass, Tool.getGsoapProxyName(repository, startingEnergisticsResqml2_0_1Class), getterName + energisticsResqml2_0_1Connector.SupplierEnd.Role + "Count", "unsigned int",
                                resqml2_0_1Path + "->" + energisticsResqml2_0_1Connector.SupplierEnd.Role,
                                "res = " + resqml2_0_1Path + "->" + energisticsResqml2_0_1Connector.SupplierEnd.Role + ".size()") == null)
                            {
                                Tool.log(repository, "Unable to properly add count getter for the " + energisticsResqml2_0_1Connector.SupplierEnd.Role + " relation of the fesapi/Class Model/" + repository.GetPackageByID(fesapiClass.PackageID).Name + "/" + fesapiClass.Name + "!");
                            }
                        }
                    }
                }
                // on traite ensuite les relations de type generalisation
                else if (energisticsResqml2_0_1Connector.Type == "Generalization")
                {
                    EA.Connector energisticsResqml2_2Connector = null;
                    foreach (EA.Connector currentEnergisticsResqml2_2Connector in energisticsResqml2_2Class.Connectors)
                    {
                        if (currentEnergisticsResqml2_2Connector.Type == "Generalization" &&
                            (repository.GetElementByID(currentEnergisticsResqml2_2Connector.SupplierID).Name == repository.GetElementByID(energisticsResqml2_0_1Connector.SupplierID).Name || (repository.GetElementByID(currentEnergisticsResqml2_2Connector.SupplierID).Name == "AbstractObject" && repository.GetElementByID(energisticsResqml2_0_1Connector.SupplierID).Name == "AbstractResqmlDataObject")) &&
                            repository.GetElementByID(currentEnergisticsResqml2_2Connector.ClientID).Name == repository.GetElementByID(energisticsResqml2_0_1Connector.ClientID).Name)
                        {
                            energisticsResqml2_2Connector = currentEnergisticsResqml2_2Connector;
                            break;
                        }
                    }

                    if (energisticsResqml2_2Connector != null && Tool.isTopLevelClass(energisticsResqml2_0_1Class) && Tool.isTopLevelClass(energisticsResqml2_2Class))
                    {
                        continue;
                    }
                    else if (energisticsResqml2_2Connector != null && !Tool.isTopLevelClass(energisticsResqml2_0_1Class) && !Tool.isTopLevelClass(energisticsResqml2_2Class))
                    {
                        // par construction, si ce n'est pas marké côté Resqml 2.0.1, ce n'est pas non pluc marqué côté Resqml 2.2
                        // Attention toutefois à bien marquer les 2 dans le if dans le cas ou on appelerai récursivement la version non Resqml2
                        if (!markedGeneralizationSet.Contains(energisticsResqml2_0_1Connector.ConnectorID.ToString()))
                        {
                            if (energisticsResqml2_0_1Connector.ClientID == energisticsResqml2_0_1Class.ElementID)
                            {
                                exploreResqml2RelationSetRec(
                                    fesapiClass,
                                    startingEnergisticsResqml2_0_1Class, startingEnergisticsResqml2_2Class,
                                    repository.GetElementByID(energisticsResqml2_0_1Connector.SupplierID), repository.GetElementByID(energisticsResqml2_2Connector.SupplierID),
                                    markedGeneralizationSet + (energisticsResqml2_0_1Connector.ConnectorID) + "." + (energisticsResqml2_2Connector.ConnectorID) + ".",
                                    resqml2_0_1Index, resqml2_2Index,
                                    getterName,
                                    resqml2_0_1Path, resqml2_2Path);
                            }
                            else
                            {
                                exploreResqml2RelationSetRec(
                                    fesapiClass,
                                    startingEnergisticsResqml2_0_1Class, startingEnergisticsResqml2_2Class,
                                    repository.GetElementByID(energisticsResqml2_0_1Connector.ClientID), repository.GetElementByID(energisticsResqml2_2Connector.ClientID),
                                    markedGeneralizationSet + (energisticsResqml2_0_1Connector.ConnectorID) + "." + (energisticsResqml2_2Connector.ConnectorID) + ".",
                                    resqml2_0_1Index, resqml2_2Index,
                                    getterName + repository.GetElementByID(energisticsResqml2_0_1Connector.ClientID).Name,
                                    "static_cast<" + Tool.getGsoapName(repository, repository.GetElementByID(energisticsResqml2_0_1Connector.ClientID)) + "*>(" + resqml2_0_1Path + ")",
                                    "static_cast<" + Tool.getGsoapName(repository, repository.GetElementByID(energisticsResqml2_2Connector.ClientID)) + "*>(" + resqml2_2Path + ")");
                            }
                        }
                    } 
                    else if (energisticsResqml2_2Connector != null && Tool.isTopLevelClass(energisticsResqml2_0_1Class))
                    {
                        if (!markedGeneralizationSet.Contains(energisticsResqml2_2Connector.ConnectorID.ToString()))
                        {
                            EA.Element fesapiResqml2_2Class = fesapiResqml2_2ClassList.Find(c => c.Name.Equals(fesapiClass.Name));
                            if (fesapiResqml2_2Class == null)
                            {
                                fesapiResqml2_2Class = fesapiClass;
                            }

                            if (energisticsResqml2_2Connector.ClientID == energisticsResqml2_2Class.ElementID)
                            {
                                 exploreRelationSetRec(
                                    fesapiResqml2_2Class,
                                    startingEnergisticsResqml2_2Class,
                                    repository.GetElementByID(energisticsResqml2_2Connector.SupplierID),
                                    markedGeneralizationSet + (energisticsResqml2_2Connector.ConnectorID) + ".",
                                    resqml2_2Index,
                                    getterName,
                                    resqml2_2Path);
                            }
                            else
                            {
                                exploreRelationSetRec(
                                    fesapiResqml2_2Class,
                                    startingEnergisticsResqml2_2Class,
                                    repository.GetElementByID(energisticsResqml2_2Connector.ClientID),
                                    markedGeneralizationSet + (energisticsResqml2_2Connector.ConnectorID) + ".",
                                    resqml2_2Index,
                                    getterName + repository.GetElementByID(energisticsResqml2_2Connector.ClientID).Name,
                                    "static_cast<" + Tool.getGsoapName(repository, repository.GetElementByID(energisticsResqml2_2Connector.ClientID)) + "*>(" + resqml2_2Path + ")");
                            }
                        }
                    }
                    else if (!Tool.isTopLevelClass(energisticsResqml2_0_1Class))
                    {
                        // par construction, si ce n'est pas marké côté Resqml 2.0.1, ce n'est pas non pluc marqué côté Resqml 2.2
                        // Attention toutefois à bien marquer les 2 dans le if dans le cas ou on appelerai récursivement la version non Resqml2
                        if (!markedGeneralizationSet.Contains(energisticsResqml2_0_1Connector.ConnectorID.ToString()))
                        {
                            EA.Element fesapiResqml2_0_1Class = fesapiResqml2_0_1ClassList.Find(c => c.Name.Equals(fesapiClass.Name));
                            if (fesapiResqml2_0_1Class == null)
                            {
                                fesapiResqml2_0_1Class = fesapiClass;
                            }

                            if (energisticsResqml2_0_1Connector.ClientID == energisticsResqml2_0_1Class.ElementID)
                            {
                                exploreRelationSetRec(
                                    fesapiResqml2_0_1Class,
                                    startingEnergisticsResqml2_0_1Class,
                                    repository.GetElementByID(energisticsResqml2_0_1Connector.SupplierID),
                                    markedGeneralizationSet + (energisticsResqml2_0_1Connector.ConnectorID) + ".",
                                    resqml2_0_1Index,
                                    getterName,
                                    resqml2_0_1Path);
                            }
                            else
                            {
                                exploreRelationSetRec(
                                    fesapiResqml2_0_1Class,
                                    startingEnergisticsResqml2_0_1Class,
                                    repository.GetElementByID(energisticsResqml2_0_1Connector.ClientID),
                                    markedGeneralizationSet + (energisticsResqml2_0_1Connector.ConnectorID) + ".",
                                    resqml2_0_1Index,
                                    getterName + repository.GetElementByID(energisticsResqml2_0_1Connector.ClientID).Name,
                                    "static_cast<" + Tool.getGsoapName(repository, repository.GetElementByID(energisticsResqml2_0_1Connector.ClientID)) + "*>(" + resqml2_0_1Path + ")");
                            }
                        }
                    }
                }
            }

            // on regarde ensuite les relations côté Resqml 2.2
            foreach (EA.Connector energisticsResqml2_2Connector in energisticsResqml2_2Class.Connectors)
            {
                // on traite d'abord le cas des connecteurs hors generalisation
                if (energisticsResqml2_2Connector.Type != "Generalization" && energisticsResqml2_2Connector.ClientID == energisticsResqml2_2Class.ElementID)
                {
                    //Tool.log(repository, "DEBUG: " + energisticsResqml2_0_1Class.Name + "-" + energisticsResqml2_2Connector.SupplierEnd.Role + "->" + repository.GetElementByID(energisticsResqml2_2Connector.SupplierID).Name);

                    EA.Connector energisticsResqml2_0_1Connector = null;
                    foreach (EA.Connector currentEnergisticsResqml2_0_1Connector in energisticsResqml2_0_1Class.Connectors)
                    {
                        if (currentEnergisticsResqml2_0_1Connector.Type != "Generalization" &&
                            currentEnergisticsResqml2_0_1Connector.ClientID == energisticsResqml2_0_1Class.ElementID &&
                            currentEnergisticsResqml2_0_1Connector.SupplierEnd.Role == energisticsResqml2_2Connector.SupplierEnd.Role &&
                            Tool.areSameCardinality(currentEnergisticsResqml2_0_1Connector.SupplierEnd.Cardinality, energisticsResqml2_2Connector.SupplierEnd.Cardinality))
                        {
                            energisticsResqml2_0_1Connector = currentEnergisticsResqml2_0_1Connector;
                            break;
                        }
                    }

                    if (energisticsResqml2_0_1Connector == null)
                    {
                        string newResqml2_2Path;
                        uint newResqml2_2Index = resqml2_2Index;
                        bool isResqml2_2MultipleCardinality = false;
                        if (energisticsResqml2_2Connector.SupplierEnd.Cardinality == "*" || energisticsResqml2_2Connector.SupplierEnd.Cardinality == "0..*" || energisticsResqml2_2Connector.SupplierEnd.Cardinality == "1..*")
                        {
                            newResqml2_2Path = resqml2_2Path + "->" + energisticsResqml2_2Connector.SupplierEnd.Role + "[index" + resqml2_2Index + "]";
                            newResqml2_2Index++;
                            isResqml2_2MultipleCardinality = true;
                        }
                        else if (energisticsResqml2_2Connector.SupplierEnd.Cardinality == "0..1")
                        {
                            // TODO: à tester
                            newResqml2_2Path = resqml2_2Path + "->{" + energisticsResqml2_2Connector.SupplierEnd.Role + "}";
                        }else
                        {
                            newResqml2_2Path = resqml2_2Path + "->" + energisticsResqml2_2Connector.SupplierEnd.Role;
                        }

                        EA.Element fesapiResqml2_2Class = fesapiResqml2_2ClassList.Find(c => c.Name.Equals(fesapiClass.Name));
                        if (fesapiResqml2_2Class == null)
                        {
                            fesapiResqml2_2Class = fesapiClass;
                        }

                        exploreRelationSetRec(
                            fesapiResqml2_2Class,
                            startingEnergisticsResqml2_2Class,
                            repository.GetElementByID(energisticsResqml2_2Connector.SupplierID),
                            markedGeneralizationSet,
                            newResqml2_2Index,
                            getterName + energisticsResqml2_2Connector.SupplierEnd.Role,
                            newResqml2_2Path);

                        if (isResqml2_2MultipleCardinality)
                        {
                            if (addResqmlGetter(fesapiClass, Tool.getGsoapProxyName(repository, startingEnergisticsResqml2_2Class), getterName + energisticsResqml2_2Connector.SupplierEnd.Role + "Count", "unsigned int",
                                resqml2_2Path + "->" + energisticsResqml2_2Connector.SupplierEnd.Role,
                                "res = " + resqml2_2Path + "->" + energisticsResqml2_2Connector.SupplierEnd.Role + ".size()") == null)
                            {
                                Tool.log(repository, "Unable to properly add count getter for the " + energisticsResqml2_2Connector.SupplierEnd.Role + " relation of the fesapi/Class Model/" + repository.GetPackageByID(fesapiClass.PackageID).Name + "/" + fesapiClass.Name + "!");
                            }
                        }
                    }
                }
                else if (energisticsResqml2_2Connector.Type == "Generalization")
                {
                    EA.Connector energisticsResqml2_0_1Connector = null;
                    foreach (EA.Connector currentEnergisticsResqml2_0_1Connector in energisticsResqml2_0_1Class.Connectors)
                    {
                        if (currentEnergisticsResqml2_0_1Connector.Type == "Generalization" &&
                            (repository.GetElementByID(currentEnergisticsResqml2_0_1Connector.SupplierID).Name == repository.GetElementByID(energisticsResqml2_2Connector.SupplierID).Name || (repository.GetElementByID(currentEnergisticsResqml2_0_1Connector.SupplierID).Name == "AbstractObject" && repository.GetElementByID(energisticsResqml2_2Connector.SupplierID).Name == "AbstractResqmlDataObject")) &&
                            repository.GetElementByID(currentEnergisticsResqml2_0_1Connector.ClientID).Name == repository.GetElementByID(energisticsResqml2_2Connector.ClientID).Name)
                        {
                            energisticsResqml2_0_1Connector = currentEnergisticsResqml2_0_1Connector;
                            break;
                        }
                    }

                    if (energisticsResqml2_0_1Connector == null && !Tool.isTopLevelClass(energisticsResqml2_2Class))
                    {
                        if (!markedGeneralizationSet.Contains(energisticsResqml2_2Connector.ConnectorID.ToString()))
                        {
                            EA.Element fesapiResqml2_2Class = fesapiResqml2_2ClassList.Find(c => c.Name.Equals(fesapiClass.Name));
                            if (fesapiResqml2_2Class == null)
                            {
                                fesapiResqml2_2Class = fesapiClass;
                            }

                            if (energisticsResqml2_2Connector.ClientID == energisticsResqml2_2Class.ElementID)
                            {
                                exploreRelationSetRec(
                                    fesapiResqml2_2Class,
                                    startingEnergisticsResqml2_2Class,
                                    repository.GetElementByID(energisticsResqml2_2Connector.SupplierID),
                                    markedGeneralizationSet + (energisticsResqml2_2Connector.ConnectorID) + ".",
                                    resqml2_2Index,
                                    getterName,
                                    resqml2_2Path);
                            }
                            else
                            {
                                exploreRelationSetRec(
                                    fesapiResqml2_2Class,
                                    startingEnergisticsResqml2_2Class,
                                    repository.GetElementByID(energisticsResqml2_2Connector.ClientID),
                                    markedGeneralizationSet + (energisticsResqml2_2Connector.ConnectorID) + ".",
                                    resqml2_2Index,
                                    getterName + repository.GetElementByID(energisticsResqml2_2Connector.ClientID).Name,
                                    "static_cast<" + Tool.getGsoapName(repository, repository.GetElementByID(energisticsResqml2_2Connector.ClientID)) + "*>(" + resqml2_2Path + ")");
                            }
                        }
                    }
                }
            }
        }

        private void exploreRelationSet(EA.Element fesapiClass, EA.Element energisticsClass)
        {
            Tool.log(repository, "========================");
            Tool.log(repository, "begin exploreRelationSetBis...");

            Tool.log(repository, "class: " + energisticsClass.Name);
            exploreRelationSetRec(fesapiClass, energisticsClass, energisticsClass, "", 0, "get", "static_cast<" + Tool.getGsoapName(repository, energisticsClass) + "*>(" + Tool.getGsoapProxyName(repository, energisticsClass) + ")");

            Tool.log(repository, "... end exploreRelationSetBis");
            Tool.log(repository, "========================");
        }

        private void exploreRelationSetRec(EA.Element fesapiClass, EA.Element startingEnergisticsClass, EA.Element energisticsClass, string markedGeneralizationSet, uint index, string getterName, string path)
        {
            // je le chemin me mène à un top level je m'arrête
            if ((Tool.isTopLevelClass(energisticsClass) || energisticsClass.Name.Equals("AbstractResqmlDataObject") || energisticsClass.Name.Equals("AbstractObject")) && path != "static_cast<" + Tool.getGsoapName(repository, startingEnergisticsClass) + "*>(" + Tool.getGsoapProxyName(repository, startingEnergisticsClass) + ")") //energisticsClass != startingEnergisticsClass) // path != "gsoapProxy")
            {
                Tool.log(repository, "TopLevelObject relation (" + Tool.getFesapiNamespace(repository, energisticsClass) + "): " + path);
                return;
            }

            // j'explore tout les attributs de la classe courante
            foreach (EA.Attribute attribute in energisticsClass.Attributes)
            {
                string newPath;
                // si la multiplicité est supérieur à 1
                if (!(attribute.UpperBound == "1"))
                {
                    Tool.log(repository, "Attribute relation (" + Tool.getFesapiNamespace(repository, energisticsClass) + "): " + path + "->" + attribute.Name + "[index" + index + "]");
                    newPath = path + "->" + attribute.Name + "[index" + index + "]";
                }
                // sinon si l'attribut est optionnel
                else if (attribute.LowerBound == "0")
                {
                    Tool.log(repository, "Attribute relation (" + Tool.getFesapiNamespace(repository, energisticsClass) + "): " + path + "->(*" + attribute.Name + ")");
                    newPath = path + "->(*" + attribute.Name + ")";
                }
                // sinon s'il est unique et mandatory
                else
                {
                    Tool.log(repository, "Attribute relation (" + Tool.getFesapiNamespace(repository, energisticsClass) + "): " + path + "->" + attribute.Name);
                    newPath = path + "->" + attribute.Name;
                }

                addResqmlGetterSet(fesapiClass, attribute, getterName + attribute.Name, newPath);
            }

            foreach (EA.Connector connector in energisticsClass.Connectors)
            {
                if (connector.Type != "Generalization" && connector.ClientID == energisticsClass.ElementID)
                {
                    string newPath;
                    uint newIndex = index;
                    bool isMultipleCardinality = false;

                    if (connector.SupplierEnd.Cardinality == "*" || connector.SupplierEnd.Cardinality == "0..*" || connector.SupplierEnd.Cardinality == "1..*")
                    {
                        newPath = path + "->" + connector.SupplierEnd.Role + "[index" + index + "]";
                        newIndex++;
                        isMultipleCardinality = true;
                    }
                    else if (connector.SupplierEnd.Cardinality == "0..1")
                    {
                        // TODO: à tester !
                        newPath = path + "->{" + connector.SupplierEnd.Role + "}";
                    }
                    else
                    {
                        newPath = path + "->" + connector.SupplierEnd.Role;
                    }

                    exploreRelationSetRec(
                        fesapiClass, 
                        startingEnergisticsClass, 
                        repository.GetElementByID(connector.SupplierID), 
                        markedGeneralizationSet, 
                        newIndex, 
                        getterName + connector.SupplierEnd.Role, 
                        newPath);

                    if (isMultipleCardinality)
                    {
                        if (addResqmlGetter(fesapiClass, Tool.getGsoapProxyName(repository, startingEnergisticsClass), getterName + connector.SupplierEnd.Role + "Count", "unsigned int",
                            path + "->" + connector.SupplierEnd.Role,
                            "res = " + path + "->" + connector.SupplierEnd.Role + ".size()") == null)
                        {
                            Tool.log(repository, "Unable to properly add count getter for the " + connector.SupplierEnd.Role + " relation of the fesapi/Class Model/" + repository.GetPackageByID(fesapiClass.PackageID).Name + "/" + fesapiClass.Name + "!");
                        }
                    }
                }
                else if (connector.Type == "Generalization")
                {
                    if (Tool.isTopLevelClass(energisticsClass))
                    {
                        continue;
                    }

                    if (!markedGeneralizationSet.Contains(connector.ConnectorID.ToString()))
                    {
                        if (connector.ClientID == energisticsClass.ElementID)
                        {
                            exploreRelationSetRec(fesapiClass, startingEnergisticsClass, repository.GetElementByID(connector.SupplierID), markedGeneralizationSet + (connector.ConnectorID) + ".", index, getterName, path);
                        }
                        else
                        {
                            exploreRelationSetRec(fesapiClass, startingEnergisticsClass, repository.GetElementByID(connector.ClientID), markedGeneralizationSet + (connector.ConnectorID) + ".", index, getterName, "static_cast<" + Tool.getGsoapName(repository, repository.GetElementByID(connector.ClientID)) + "*>(" + path + ")");
                        }
                    }
                }
            }
        }

        string secureExpression(string expression, string returnExpression)
        {
            return secureExpressionRec(expression, returnExpression, 1, 1, "\t");
        }

        string secureExpressionRec(string expression, string returnExpression, int indexOccurence, int pointerOccurence, string tabulation)
        {
            // on récupère la position du indexOccurence-ième "[index"
            int indexPos = Tool.indexOfOccurence(expression, "[index", indexOccurence);

            // on récupère la position du pointerOccurence-ième "{"
            int pointerPos = Tool.indexOfOccurence(expression, "{", pointerOccurence);

            if (indexPos == -1 && pointerPos == -1)
            {
                return tabulation + returnExpression.Replace("{", "").Replace("}", "") + ";\n";
            }
            else if (indexPos == -1 || (pointerPos != -1 && pointerPos<indexPos))
            {
                // on garde ce qui précède le "{"
                string prefix = expression.Substring(0, pointerPos);

                // on garde ce qui suit le "{"
                string suffix = expression.Substring(pointerPos + 2);

                // on extrait le nom de l'attribut pointé
                int firstClosingBracketPos = suffix.IndexOf("}");
                string attributeName = suffix.Substring(0, firstClosingBracketPos);

                // dans ce qui suit le "(*", on compte le nombre de parenthèses fermantes
                // moins le nombre de parenthèse ouvrante
                // moins 1 pour la parenthèse qui ferme celle du pointeur
                int bracketsCount = Tool.countStringOccurrences(suffix, ')'.ToString()) - Tool.countStringOccurrences(suffix, '('.ToString());

                // dans le prefix, on compte la position de la bracketCount-ieme parenthèse ouvrante
                int bracketPos = Tool.getNthIndex(prefix, '(', bracketsCount);

                // je retire de prefix tout ce qui précède (inclue) la bracketCount-ieme parenthèse ouvrante
                prefix = prefix.Remove(0, bracketPos + 1);

                string res = "";
                res += tabulation + "if (" + prefix + attributeName + " != nullptr)\n";
                res += tabulation + "{\n";
                res += secureExpressionRec(expression, returnExpression, indexOccurence, pointerOccurence + 1, tabulation + "\t");
                res += tabulation + "}\n";
                res += tabulation + "else\n";
                res += tabulation + "{\n";
                res += tabulation + "\tthrow out_of_range(\"The " + attributeName + " atribute is not defined\");\n";
                res += tabulation + "}\n";

                return res;
            }
            else
            {
                // on garde tout ce qui précède le "[index"
                string prefix = expression.Substring(0, indexPos);

                // on mémorise ce qui suit le "[index"
                string suffix = expression.Substring(indexPos + 6);

                // on y compte le nombre de parenthèses fermantes moins le nombre de parenthèse ouvrante
                int bracketsCount = Tool.countStringOccurrences(suffix, ')'.ToString()) - Tool.countStringOccurrences(suffix, '('.ToString());

                // on calcule la position de la bracketCount-ieme parenthèse ouvrante
                int bracketPos = Tool.getNthIndex(prefix, '(', bracketsCount);

                // je retire de res tout ce qui précède (inclue) la bracketCount-ieme parenthèse ouvrante
                prefix = prefix.Remove(0, bracketPos + 1);

                string res = "";
                res += tabulation + "if (index" + (indexOccurence - 1) + " < " + prefix + ".size())\n";
                res += tabulation + "{\n";
                res += secureExpressionRec(expression, returnExpression, indexOccurence + 1, pointerOccurence, tabulation + "\t");
                res += tabulation + "}\n";
                res += tabulation + "else\n";
                res += tabulation + "{\n";
                res += tabulation + "\tthrow out_of_range(\"index" + (indexOccurence - 1) + " is out of range\");\n";
                res += tabulation + "}\n";

                return res;
            }
        }
        
        void addResqmlGetterSet(EA.Element fesapiClass, EA.Attribute energisticsAttribute, string getterName, string pathToAttribute)
        {
            // get the fesapiClass package name for explciting log messages
            string packageName = repository.GetPackageByID(fesapiClass.PackageID).Name;

            string gsoapProxy = Tool.getGsoapProxyName(repository, repository.GetElementByID(energisticsAttribute.ParentID));

            // if the type of the attribute is a basic type
            string attributeBasicType = Tool.getBasicType(repository, energisticsAttribute);
            if (!(attributeBasicType.Equals("")))
            {
                if (addResqmlGetter(fesapiClass, gsoapProxy, getterName, attributeBasicType, pathToAttribute, "res = " + pathToAttribute) == null)
                {
                    Tool.log(repository, "Unable to properly add basic type getter for the " + energisticsAttribute.Name + " attribute of the fesapi/Class Model/" + packageName + "/" + fesapiClass.Name + "!");
                    return;
                }
            }
            else
            {
                EA.Element attributeType = repository.GetElementByID(energisticsAttribute.ClassifierID);
            
                // else if the type of the attribute is a measure type
                if (Tool.isMeasureType(attributeType))
                {
                    if (addResqmlGetter(fesapiClass, gsoapProxy, getterName, "double", pathToAttribute, "res = " + pathToAttribute + "->__item") == null)
                    {
                        Tool.log(repository, "Unable to properly add measure value getter for the " + energisticsAttribute.Name + " attribute of the fesapi/Class Model/" + packageName + "/" + fesapiClass.Name + "!");
                        return;
                    }

                    // TODO: à tester
                    // in order to generate unit of measure and string unit of measure getters 
                    addResqmlGetterSet(fesapiClass, attributeType.Attributes.GetAt(0), getterName + "Uom", pathToAttribute + "->uom");
                }
                // else if the type of the attribute is an enum type
                else if (Tool.isEnum(attributeType))
                {
                    if (addResqmlGetter(fesapiClass, gsoapProxy, getterName, Tool.getGsoapName(repository, attributeType), pathToAttribute, "res = " + pathToAttribute) == null)
                    {
                        Tool.log(repository, "Unable to properly add enum value getter for the " + energisticsAttribute.Name + " attribute of the fesapi/Class Model/" + packageName + "/" + fesapiClass.Name + "!");
                        return;
                    }
                    if (addResqmlGetter(fesapiClass, gsoapProxy, getterName + "AsString", "std::string", pathToAttribute, "res = " + Tool.getGsoapEnum2SConverterName(repository, attributeType) + "(" + gsoapProxy + "->soap, " + pathToAttribute + ")") == null)
                    {
                        Tool.log(repository, "Unable to properly add string enum value getter for the " + energisticsAttribute.Name + " attribute of the fesapi/Class Model/" + packageName + "/" + fesapiClass.Name + "!");
                        return;
                    }
                }
                // else if the type of the attribute is an extended enum type
                else if (attributeType.Name.EndsWith("Ext"))
                {
                    EA.Element enumType = Tool.getEnumTypeFromEnumExtType(repository, attributeType);

                    if (addResqmlGetter(fesapiClass, gsoapProxy, getterName, Tool.getGsoapName(repository, enumType), pathToAttribute, Tool.getGsoapS2EnumConverterName(repository, enumType) + "(" + gsoapProxy + "->soap, " + pathToAttribute + ".c_str(), &res)") == null)
                    {
                        Tool.log(repository, "Unable to properly add enum value getter for the " + energisticsAttribute.Name + " attribute of the fesapi/Class Model/" + packageName + "/" + fesapiClass.Name + "!");
                        return;
                    }
                    if (addResqmlGetter(fesapiClass, gsoapProxy, getterName + "AsString", "std::string", pathToAttribute, "res = " + pathToAttribute) == null)
                    {
                        Tool.log(repository, "Unable to properly add string enum value getter for the " + energisticsAttribute.Name + " attribute of the fesapi/Class Model/" + packageName + "/" + fesapiClass.Name + "!");
                        return;
                    }
                }
                else
                {
                    Tool.log(repository, "Unable to properly add getter for the " + energisticsAttribute.Name + " attribute of the fesapi/Class Model/" + packageName + "/" + fesapiClass.Name + "!");
                    return;
                }
            }

            // TODO: A tester absolument
            if (!(energisticsAttribute.UpperBound.Equals("1")))
            {
                addResqmlGetter(fesapiClass, gsoapProxy, getterName + "Count", "unsigned int",
                    pathToAttribute.Substring(0, pathToAttribute.LastIndexOf("[")),
                    "res = " + pathToAttribute.Substring(0, pathToAttribute.LastIndexOf("[")) + ".size()");
                {
                    Tool.log(repository, "Unable to properly add count getter for the " + energisticsAttribute.Name + " attribute of the fesapi/Class Model/" + packageName + "/" + fesapiClass.Name + "!");
                }
            }
        }

        EA.Method addResqmlGetter(EA.Element fesapiClass, string gsoapProxy, string getterName, string returnType, string pathToAttribute, string returnExpression)
        {
            Tool.log(repository, "adding a resqml2_0_1/resqml2_2 getter: " + getterName);

            EA.Method getter = fesapiClass.Methods.AddNew(getterName, returnType);

            getter.Code = returnType + " res;\n";
            getter.Code += "if (" + gsoapProxy + " != nullptr)\n";
            getter.Code += "{\n";
            getter.Code += secureExpression(pathToAttribute, returnExpression);
            getter.Code += "}\n";
            getter.Code += "else\n";
            getter.Code += "{\n";
            getter.Code += "\tthrow logic_error(\"Not implemented yet\");\n";
            getter.Code += "}\n";
            getter.Code += "return res;";

            getter.Stereotype = "const";
            if (!(getter.Update()))
            {
                Tool.showMessageBox(repository, getter.GetLastError());
                return null;
            }
            fesapiClass.Methods.Refresh();

            int resqml2_0_1IndexCount = Tool.countStringOccurrences(pathToAttribute, "[index");      
            for (int i = 0; i < resqml2_0_1IndexCount; i++)
            {
                EA.Parameter parameter = getter.Parameters.AddNew("index" + i, "unsigned int &");
                parameter.IsConst = true;
                parameter.Position = i;

                if (!(parameter.Update()))
                {
                    Tool.showMessageBox(repository, parameter.GetLastError());
                    return null;
                }
            }
            getter.Parameters.Refresh();

            EA.MethodTag bodyLocationTag = getter.TaggedValues.AddNew("bodyLocation", "classBody");
            if (!(bodyLocationTag.Update()))
            {
                Tool.showMessageBox(repository, bodyLocationTag.GetLastError());
                return null;
            }
            getter.TaggedValues.Refresh();

            return getter;
        }

        private void addResqml2GetterSet(EA.Element fesapiResqml2Class, EA.Attribute energisticsResqml2_0_1Attribute, EA.Attribute energisticsResqml2_2Attribute, string getterName, string resqml2_0_1PathToAttribute, string resqml2_2PathToAttribute)
        {
            // get the fesapiClass package name for expliciting log messages
            string packageName = repository.GetPackageByID(fesapiResqml2Class.PackageID).Name;

            string gsoapResqml2_0_1Proxy = Tool.getGsoapProxyName(repository, repository.GetElementByID(energisticsResqml2_0_1Attribute.ParentID));
            string gsoapResqml2_2Proxy = Tool.getGsoapProxyName(repository, repository.GetElementByID(energisticsResqml2_2Attribute.ParentID));

            string resqml2_0_1AttributeBasicType = Tool.getBasicType(repository, energisticsResqml2_0_1Attribute);
            string resqml2_2AttributeBasicType = Tool.getBasicType(repository, energisticsResqml2_2Attribute);
            // if Resqml 2.0.1 and Resqml 2.2 attributes type are basic type 
            if ((resqml2_0_1AttributeBasicType != "") && (resqml2_2AttributeBasicType != ""))
            {
                // if basic types are the same
                if (resqml2_0_1AttributeBasicType.Equals(resqml2_2AttributeBasicType))
                {
                    if (addResqml2Getter(fesapiResqml2Class, getterName, resqml2_0_1AttributeBasicType,
                        resqml2_0_1PathToAttribute,
                        resqml2_2PathToAttribute,
                        "res = " + resqml2_0_1PathToAttribute,
                        "res = " + resqml2_2PathToAttribute) == null)
                    {
                        Tool.log(repository, "Unable to properly add basic type getter for the " + energisticsResqml2_0_1Attribute.Name + " attribute of the fesapi/Class Model/" + packageName + "/" + fesapiResqml2Class.Name + "!");
                        return;
                    }
                }
                else
                {
                    Tool.log(repository, "Unable to properly add basic type getter for the " + energisticsResqml2_0_1Attribute.Name + " attribute of the fesapi/Class Model/" + packageName + "/" + fesapiResqml2Class.Name + "! Basic type differs betwwen Resqml 2.0.1 and Resqml 2.2.");
                    return;
                }
            }
            // TODO: does not handle the fact that unit of measure can be different between Resqml 2.0.1 and Resqml 2.2. Got some notes on how to handle that. 
            // this can be detected if there is no possible convertion between them (gSOAP -> s2enumName error case).
            else
            {
                EA.Element resqml2_0_1AttributeType = repository.GetElementByID(energisticsResqml2_0_1Attribute.ClassifierID);
                EA.Element resqml2_2AttributeType = repository.GetElementByID(energisticsResqml2_2Attribute.ClassifierID);

                // else if both attributes type are measure type
                if (Tool.isMeasureType(resqml2_0_1AttributeType) && Tool.isMeasureType(resqml2_2AttributeType))
                {
                    if (addResqml2Getter(fesapiResqml2Class, getterName, "double", resqml2_0_1PathToAttribute, resqml2_2PathToAttribute, "res = " + resqml2_0_1PathToAttribute + "->__item", "res = " + resqml2_2PathToAttribute + "->__item") == null)
                    {
                        Tool.log(repository, "Unable to properly add measure value getter for the " + energisticsResqml2_0_1Attribute.Name + " attribute of the fesapi/Class Model/" + packageName + "/" + fesapiResqml2Class.Name + "!");
                        return;
                    }

                    // in order to generate unit of measure and string unit of measure getters
                    EA.Attribute resqml2_0_1UomAttribute = resqml2_0_1AttributeType.Attributes.GetAt(0);
                    EA.Attribute resqml2_2UomAttribute = resqml2_2AttributeType.Attributes.GetAt(0);
                    addResqml2GetterSet(fesapiResqml2Class, resqml2_0_1UomAttribute, resqml2_2UomAttribute, getterName + "Uom", resqml2_0_1PathToAttribute + "->uom", resqml2_2PathToAttribute + "->uom");
                    
                }
                // else if both attributes are enum type
                else if (Tool.isEnum(resqml2_0_1AttributeType) && Tool.isEnum(resqml2_2AttributeType))
                {
                    if (addResqml2Getter(fesapiResqml2Class, getterName, Tool.getGsoapName(repository, resqml2_2AttributeType), 
                        resqml2_0_1PathToAttribute, 
                        resqml2_2PathToAttribute,
                        Tool.getGsoapS2EnumConverterName(repository, resqml2_2AttributeType) + "(" + gsoapResqml2_0_1Proxy + "->soap, " + Tool.getGsoapEnum2SConverterName(repository, resqml2_0_1AttributeType) + "(" + gsoapResqml2_0_1Proxy + "->soap, " + resqml2_0_1PathToAttribute + "), &res)",
                        "res = " + resqml2_2PathToAttribute
                        ) == null)
                    {
                        Tool.log(repository, "Unable to properly add enum value getter for the " + energisticsResqml2_0_1Attribute.Name + " attribute of the fesapi/Class Model/" + packageName + "/" + fesapiResqml2Class.Name + "!");
                        return;
                    }
                    if (addResqml2Getter(fesapiResqml2Class, getterName + "AsString", "std::string",
                        resqml2_0_1PathToAttribute,
                        resqml2_2PathToAttribute,
                        "res = " + Tool.getGsoapEnum2SConverterName(repository, resqml2_0_1AttributeType) + "(" + gsoapResqml2_0_1Proxy + "->soap, " + resqml2_0_1PathToAttribute + ")",
                        "res = " + Tool.getGsoapEnum2SConverterName(repository, resqml2_2AttributeType) + "(" + gsoapResqml2_2Proxy + "->soap, " + resqml2_2PathToAttribute + ")") == null)
                    {
                        Tool.log(repository, "Unable to properly add string enum value getter for the " + energisticsResqml2_0_1Attribute.Name + " attribute of the fesapi/Class Model/" + packageName + "/" + fesapiResqml2Class.Name + "!");
                        return;
                    }
                }
                // TODO: when Resqml 2.2 type ends with "Ext" it is not tested that prefix is the same than Resqml 2.0.1 type
                else if (Tool.isEnum(resqml2_0_1AttributeType) && resqml2_2AttributeType.Name.EndsWith("Ext"))
                {
                    EA.Element enumType = Tool.getEnumTypeFromEnumExtType(repository, resqml2_2AttributeType);

                    if (addResqml2Getter(fesapiResqml2Class, getterName, Tool.getGsoapName(repository, enumType),
                        resqml2_0_1PathToAttribute,
                        resqml2_2PathToAttribute,
                        Tool.getGsoapS2EnumConverterName(repository, enumType) + "(" + gsoapResqml2_0_1Proxy + "->soap, " + Tool.getGsoapEnum2SConverterName(repository, resqml2_0_1AttributeType) + "(" + gsoapResqml2_0_1Proxy + "->soap, " + resqml2_0_1PathToAttribute + "), &res)",
                        Tool.getGsoapS2EnumConverterName(repository, enumType) + "(" + gsoapResqml2_2Proxy + "->soap, " + resqml2_2PathToAttribute + ".c_str(), &res)") == null)
                    {
                        Tool.log(repository, "Unable to properly add enum value getter for the " + energisticsResqml2_0_1Attribute.Name + " attribute of the fesapi/Class Model/" + packageName + "/" + fesapiResqml2Class.Name + "!");
                        return;
                    }
                    if (addResqml2Getter(fesapiResqml2Class, getterName + "AsString", "std::string", 
                        resqml2_0_1PathToAttribute,
                        resqml2_2PathToAttribute,
                        "res = " + Tool.getGsoapEnum2SConverterName(repository, resqml2_0_1AttributeType) + "(" + gsoapResqml2_0_1Proxy + "->soap, " + resqml2_0_1PathToAttribute + ")",
                        "res = " + resqml2_2PathToAttribute) == null)
                    {
                        Tool.log(repository, "Unable to properly add string enum value getter for the " + energisticsResqml2_0_1Attribute.Name + " attribute of the fesapi/Class Model/" + packageName + "/" + fesapiResqml2Class.Name + "!");
                        return;
                    }
                }
                else
                {
                    Tool.log(repository, "Unable to properly add getter for the " + energisticsResqml2_0_1Attribute.Name + " attribute of the fesapi/Class Model/" + packageName + "/" + fesapiResqml2Class.Name + "!");
                    return;
                }
            }

            if (!(energisticsResqml2_0_1Attribute.UpperBound.Equals("1")) && !(energisticsResqml2_2Attribute.UpperBound.Equals("1")))
            {
                if (addResqml2Getter(fesapiResqml2Class, getterName + "Count", "unsigned int",
                    resqml2_0_1PathToAttribute.Substring(0, resqml2_0_1PathToAttribute.LastIndexOf("[")),
                    resqml2_2PathToAttribute.Substring(0, resqml2_2PathToAttribute.LastIndexOf("[")),
                    "res = " + resqml2_0_1PathToAttribute.Substring(0, resqml2_0_1PathToAttribute.LastIndexOf("[")) + ".size()",
                    "res = " + resqml2_2PathToAttribute.Substring(0, resqml2_2PathToAttribute.LastIndexOf("[")) + ".size()") == null)
                {
                    Tool.log(repository, "Unable to properly add count getter for the " + energisticsResqml2_0_1Attribute.Name + " attribute of the fesapi/Class Model/" + packageName + "/" + fesapiResqml2Class.Name + "!");
                }
            }
            else if (!(energisticsResqml2_0_1Attribute.UpperBound.Equals("1")))
            {
                if (addResqml2Getter(fesapiResqml2Class, getterName + "Count", "unsigned int",
                    resqml2_0_1PathToAttribute.Substring(0, resqml2_0_1PathToAttribute.LastIndexOf("[")),
                    "",
                    "res = " + resqml2_0_1PathToAttribute.Substring(0, resqml2_0_1PathToAttribute.LastIndexOf("[")) + ".size()",
                    "res = 1") == null)
                {
                    Tool.log(repository, "Unable to properly add count getter for the " + energisticsResqml2_0_1Attribute.Name + " attribute of the fesapi/Class Model/" + packageName + "/" + fesapiResqml2Class.Name + "!");
                }
            }
            else if (!(energisticsResqml2_2Attribute.UpperBound.Equals("1")))
            {
                if (addResqml2Getter(fesapiResqml2Class, getterName + "Count", "unsigned int",
                    "",
                    resqml2_2PathToAttribute.Substring(0, resqml2_2PathToAttribute.LastIndexOf("[")),
                    "res = 1",
                    "res = " + resqml2_2PathToAttribute.Substring(0, resqml2_2PathToAttribute.LastIndexOf("[")) + ".size()") == null)
                {
                    Tool.log(repository, "Unable to properly add count getter for the " + energisticsResqml2_0_1Attribute.Name + " attribute of the fesapi/Class Model/" + packageName + "/" + fesapiResqml2Class.Name + "!");
                }
            }
        }

        EA.Method addResqml2Getter(EA.Element fesapiClass, string getterName, string returnType, string resqml2_0_1PathToAttribute, string resqml2_2PathToAttribute, string resqml2_0_1ReturnExpression, string resqml2_2ReturnExpression)
        {
            Tool.log(repository, "adding a resqml2 getter: " + getterName);

            EA.Method getter = fesapiClass.Methods.AddNew(getterName, returnType);

            getter.Code = returnType + " res;\n";
            getter.Code += "if (gsoapProxy2_0_1 != nullptr)\n";
            getter.Code += "{\n";
            getter.Code += secureExpression(resqml2_0_1PathToAttribute, resqml2_0_1ReturnExpression);
            getter.Code += "}\n";
            getter.Code += "else if (gsoapProxy2_2 != nullptr)\n";
            getter.Code += "{\n";
            getter.Code += secureExpression(resqml2_2PathToAttribute, resqml2_2ReturnExpression);
            getter.Code += "}\n";
            getter.Code += "else\n";
            getter.Code += "{\n";
            getter.Code += "\tthrow logic_error(\"Not implemented yet\");\n";
            getter.Code += "}\n";
            getter.Code += "return res;";

            getter.Stereotype = "const";
            if (!(getter.Update()))
            {
                Tool.showMessageBox(repository, getter.GetLastError());
                return null;
            }
            fesapiClass.Methods.Refresh();

            int resqml2_0_1IndexCount = Tool.countStringOccurrences(resqml2_0_1PathToAttribute, "index");
            int resqml2_2IndexCount = Tool.countStringOccurrences(resqml2_2PathToAttribute, "index");
            int maxIndexCount = System.Math.Max(resqml2_0_1IndexCount, resqml2_2IndexCount);
            int minIndexCount = System.Math.Min(resqml2_0_1IndexCount, resqml2_2IndexCount);
            for (int i = 0; i < minIndexCount; i++)
            {
                EA.Parameter parameter = getter.Parameters.AddNew("index" + i, "unsigned int &");
                parameter.IsConst = true;
                parameter.Position = i;
                    
                if (!(parameter.Update()))
                {
                    Tool.showMessageBox(repository, parameter.GetLastError());
                    return null;
                }

                getter.Parameters.Refresh();
            }

            for (int i = minIndexCount; i < maxIndexCount; i++)
            {
                EA.Parameter parameter = getter.Parameters.AddNew("index" + i, "unsigned int &");
                parameter.IsConst = true;
                parameter.Default = "0";
                parameter.Position = i;

                if (!(parameter.Update()))
                {
                    Tool.showMessageBox(repository, parameter.GetLastError());
                    return null;
                }

                getter.Parameters.Refresh();
            }

            EA.MethodTag bodyLocationTag = getter.TaggedValues.AddNew("bodyLocation", "classBody");
            if (!(bodyLocationTag.Update()))
            {
                Tool.showMessageBox(repository, bodyLocationTag.GetLastError());
                return null;
            }
            getter.TaggedValues.Refresh();

            return getter;
        }

        #endregion

        #endregion
    }
}
