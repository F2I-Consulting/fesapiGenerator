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

        /// <summary>
        /// A list associating fesapi classes (id) to sets of in-backward-relation fesapi classes (ids)
        /// </summary>
        private SortedList<int, SortedSet<int>> backwardRelationSet = null;

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

            backwardRelationSet = new SortedList<int, SortedSet<int>>();
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
                //if (!(energisticsClassName.Equals("AbstractLocal3dCrs")) && !(energisticsClassName.Equals("LocalDepth3dCrs")) && !(energisticsClassName.Equals("LocalTime3dCrs")) && !(energisticsClassName.Equals("Activity")))
                //if (!(energisticsClassName.Equals("AbstractRepresentation")) && !(energisticsClassName.Equals("AbstractFeatureInterpretation")) && !(energisticsClassName.Equals("AbstractFeature")) && !(energisticsClassName.Equals("GeneticBoundaryFeature")) && !(energisticsClassName.Equals("BoundaryFeatureInterpretation")))
                if (!energisticsClassName.Equals("AbstractClass1") && !energisticsClassName.Equals("AbstractClass2") && !energisticsClassName.Equals("Class1") && !energisticsClassName.Equals("Class2"))
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
                //if (!(energisticsClassName.Equals("AbstractLocal3dCrs")) && !(energisticsClassName.Equals("LocalDepth3dCrs")) && !(energisticsClassName.Equals("LocalTime3dCrs")) && !(energisticsClassName.Equals("Activity")))
                //if (!(energisticsClassName.Equals("AbstractRepresentation")) && !(energisticsClassName.Equals("AbstractFeatureInterpretation")) && !(energisticsClassName.Equals("AbstractFeature")) && !(energisticsClassName.Equals("GeneticBoundaryFeature")) && !(energisticsClassName.Equals("BoundaryFeatureInterpretation")))
                if (!energisticsClassName.Equals("AbstractClass1") && !energisticsClassName.Equals("AbstractClass2") && !energisticsClassName.Equals("Class1") && !energisticsClassName.Equals("Class2"))
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

                exploreModelFromCommonClass(fesapiResqml2Class, energisticsResqml2_0_1Class, energisticsResqml2_2Class);
            }
            foreach (EA.Element fesapiResqml2_0_1Class in fesapiResqml2_0_1ClassList)
            {
                EA.Element fesapiResqml2Class = fesapiResqml2ClassList.Find(c => c.Name.Equals(fesapiResqml2_0_1Class.Name));
                if (fesapiResqml2Class == null)
                {
                    EA.Element energisticsResqml2_0_1Class = fesapiResqml2_0_1toEnergisticsResqml2_0_1[fesapiResqml2_0_1Class];
                    exploreModel(fesapiResqml2_0_1Class, energisticsResqml2_0_1Class);
                }
            }
            foreach (EA.Element fesapiResqml2_2Class in fesapiResqml2_2ClassList)
            {
                EA.Element fesapiResqml2Class = fesapiResqml2ClassList.Find(c => c.Name.Equals(fesapiResqml2_2Class.Name));
                if (fesapiResqml2Class == null)
                {
                    EA.Element energisticsResqml2_2Class = fesapiResqml2_2toEnergisticsResqml2_2[fesapiResqml2_2Class];
                    exploreModel(fesapiResqml2_2Class, energisticsResqml2_2Class);
                }
            }

            // make sure the model view is up to date in the Enterprise Architect GUI
            repository.RefreshModelView(0);
        }

        #endregion

        #region private methods

        private EA.TaggedValue addOrUpdateFesapiIncludeTag(EA.Element fesapiClass, string value)
        {
            EA.TaggedValue fesapiIncludeTag = fesapiClass.TaggedValues.GetByName(Constants.fesapiIncludeTag);

            if (fesapiIncludeTag == null)
            {
                fesapiIncludeTag = fesapiClass.TaggedValues.AddNew(Constants.fesapiIncludeTag, "");
            }

            fesapiIncludeTag.Value += value;
            if (!fesapiIncludeTag.Update())
            {
                Tool.showMessageBox(repository, fesapiIncludeTag.GetLastError());
                return null;
            }

            return fesapiIncludeTag;
        }

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

        /// <summary>
        /// Common does not mean "common" namespace but class belonging to "fesapi/resqml2" namespace
        /// </summary>
        /// <param name="fesapiClass"></param>
        /// <param name="energisticsResqml2_0_1Class"></param>
        /// <param name="energisticsResqml2_2Class"></param>
        private void exploreModelFromCommonClass(EA.Element fesapiClass, EA.Element energisticsResqml2_0_1Class, EA.Element energisticsResqml2_2Class)
        {
            Tool.log(repository, "========================");
            Tool.log(repository, "begin exploreResqml2RelationSet...");

            Tool.log(repository, "class: " + energisticsResqml2_0_1Class.Name);

            SortedSet<int> markedGeneralizationSet = new SortedSet<int>();
            exploreModelFromCommonClassRec(
                fesapiClass,
                energisticsResqml2_0_1Class,
                energisticsResqml2_2Class,
                energisticsResqml2_0_1Class, 
                energisticsResqml2_2Class,
                markedGeneralizationSet,
                0, 0,
                "",
                "static_cast<" + Tool.getGsoapName(repository, energisticsResqml2_0_1Class) + "*>(" + Tool.getGsoapProxyName(repository, energisticsResqml2_0_1Class) + ")",
                "static_cast<" + Tool.getGsoapName(repository, energisticsResqml2_2Class) + "*>(" + Tool.getGsoapProxyName(repository, energisticsResqml2_2Class) + ")");

            Tool.log(repository, "... end exploreResqml2RelationSet");
            Tool.log(repository, "========================");
        }

        private void exploreModelFromCommonClassRec(
            EA.Element startingFesapiClass,
            EA.Element startingEnergisticsResqml2_0_1Class,
            EA.Element startingEnergisticsResqml2_2Class,
            EA.Element currentEnergisticsResqml2_0_1Class,
            EA.Element currentEnergisticsResqml2_2Class,
            //string markedGeneralizationSet,
            SortedSet<int> markedGeneralizationSet,
            uint resqml2_0_1Index,
            uint resqml2_2Index,
            string pathName,
            string resqml2_0_1AttributeAccessExpression,
            string resqml2_2AttributeAccessExpression)
        {
            //if (resqml2_0_1Path != "gsoapProxy2_0_1" && resqml2_2Path != "gsoapProxy2_2")
            //if (energisticsResqml2_0_1Class != startingEnergisticsResqml2_0_1Class && energisticsResqml2_2Class != startingEnergisticsResqml2_2Class)
            if (resqml2_0_1AttributeAccessExpression != "static_cast<" + Tool.getGsoapName(repository, startingEnergisticsResqml2_0_1Class) + "*>(" + Tool.getGsoapProxyName(repository, startingEnergisticsResqml2_0_1Class) + ")" && resqml2_2AttributeAccessExpression != "static_cast<" + Tool.getGsoapName(repository, startingEnergisticsResqml2_2Class) + "*>(" + Tool.getGsoapProxyName(repository, startingEnergisticsResqml2_2Class) + ")")
            {
                if ((Tool.isTopLevelClass(currentEnergisticsResqml2_0_1Class) || currentEnergisticsResqml2_0_1Class.Name == "AbstractResqmlDataObject") && (Tool.isTopLevelClass(currentEnergisticsResqml2_0_1Class) || currentEnergisticsResqml2_0_1Class.Name == "AbstractObject"))
                {
                    Tool.log(repository, "TopLevelObject relation (resqml2): ");
                    Tool.log(repository, "- Resqml 2.0.1 code: " + resqml2_0_1AttributeAccessExpression);
                    Tool.log(repository, "- Resqml 2.2 code: " + resqml2_2AttributeAccessExpression);

                    EA.Element fesapiResqml2DestClass = fesapiResqml2ClassList.Find(c => c.Name == currentEnergisticsResqml2_0_1Class.Name);
                    if (fesapiResqml2DestClass != null)
                    {
                        generateTopLevelRelation(startingFesapiClass, fesapiResqml2DestClass, startingEnergisticsResqml2_0_1Class, startingEnergisticsResqml2_2Class, resqml2_0_1AttributeAccessExpression, resqml2_2AttributeAccessExpression, pathName);
                    }
                    else
                    {
                        Tool.log(repository, "Not able to handle " + startingFesapiClass.Name + " to " + currentEnergisticsResqml2_0_1Class.Name + " relation. Destination class is missing from fesapi/resqml2!");
                    }
                    return;
                }
                else if ((Tool.isTopLevelClass(currentEnergisticsResqml2_0_1Class) || currentEnergisticsResqml2_0_1Class.Name == "AbstractResqmlDataObject") || (Tool.isTopLevelClass(currentEnergisticsResqml2_0_1Class) || currentEnergisticsResqml2_0_1Class.Name == "AbstractObject"))
                {
                    EA.Element fesapiResqml2_0_1Class = fesapiResqml2_0_1ClassList.Find(c => c.Name.Equals(startingFesapiClass.Name));
                    if (fesapiResqml2_0_1Class != null)
                    {
                        exploreModelRec(fesapiResqml2_0_1Class, startingEnergisticsResqml2_0_1Class, currentEnergisticsResqml2_0_1Class, markedGeneralizationSet, resqml2_0_1Index, pathName, resqml2_0_1AttributeAccessExpression);
                    }
                    else
                    {
                        exploreModelRec(startingFesapiClass, startingEnergisticsResqml2_0_1Class, currentEnergisticsResqml2_0_1Class, markedGeneralizationSet, resqml2_0_1Index, pathName, resqml2_0_1AttributeAccessExpression);
                    }

                    EA.Element fesapiResqml2_2Class = fesapiResqml2_2ClassList.Find(c => c.Name.Equals(startingFesapiClass.Name));
                    if (fesapiResqml2_2Class != null)
                    {
                        exploreModelRec(fesapiResqml2_2Class, startingEnergisticsResqml2_2Class, currentEnergisticsResqml2_2Class, markedGeneralizationSet, resqml2_2Index, pathName, resqml2_2AttributeAccessExpression);
                    }
                    else
                    {
                        exploreModelRec(startingFesapiClass, startingEnergisticsResqml2_2Class, currentEnergisticsResqml2_2Class, markedGeneralizationSet, resqml2_2Index, pathName, resqml2_2AttributeAccessExpression);
                    }
                }
            }

            #region Attributes

            // on regarde tous les attributs côté Resqml 2.0.1
            foreach (EA.Attribute energisticsResqml2_0_1Attribute in currentEnergisticsResqml2_0_1Class.Attributes)
            {
                EA.Attribute energisticsResqml2_2Attribute = null;
                foreach (EA.Attribute currentEnergisticsResqml2_2Attribute in currentEnergisticsResqml2_2Class.Attributes)
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
                    newResqml2_0_1Path = resqml2_0_1AttributeAccessExpression + "->" + energisticsResqml2_0_1Attribute.Name + "[index" + resqml2_0_1Index + "]";
                }
                // sinon si l'attribut est optionnel
                else if (energisticsResqml2_0_1Attribute.LowerBound == "0")
                {
                    newResqml2_0_1Path = resqml2_0_1AttributeAccessExpression + "->(*" + energisticsResqml2_0_1Attribute.Name + ")";
                }
                // sinon s'il est unique et mandatory
                else
                {
                    newResqml2_0_1Path = resqml2_0_1AttributeAccessExpression + "->" + energisticsResqml2_0_1Attribute.Name;
                }

                if (energisticsResqml2_2Attribute != null)
                {
                    // *******************************
                    // on traite le code version 2.2

                    string newResqml2_2Path;
                    // si la multiplicité est supérieur à 1
                    if (!(energisticsResqml2_2Attribute.UpperBound == "1"))
                    {
                        newResqml2_2Path = resqml2_2AttributeAccessExpression + "->" + energisticsResqml2_2Attribute.Name + "[index" + resqml2_2Index + "]";
                    }
                    // sinon si l'attribut est optionnel
                    else if (energisticsResqml2_2Attribute.LowerBound == "0")
                    {
                        newResqml2_2Path = resqml2_2AttributeAccessExpression + "->(*" + energisticsResqml2_2Attribute.Name + ")";
                    }
                    // sinon s'il est unique et mandatory
                    else
                    {
                        newResqml2_2Path = resqml2_2AttributeAccessExpression + "->" + energisticsResqml2_2Attribute.Name;
                    }

                    Tool.log(repository, "Attribute relation (resqml2): " + pathName + energisticsResqml2_0_1Attribute.Name);
                    Tool.log(repository, "- Resqml 2.0.1 code: " + newResqml2_0_1Path);
                    Tool.log(repository, "- Resqml 2.2 code: " + newResqml2_2Path);

                    generateResqml2GetterSet(startingFesapiClass, energisticsResqml2_0_1Attribute, energisticsResqml2_2Attribute, pathName + energisticsResqml2_0_1Attribute.Name, newResqml2_0_1Path, newResqml2_2Path);
                }
                else
                {
                    Tool.log(repository, "Attribute relation (resqml2_0_1): " + pathName + energisticsResqml2_0_1Attribute.Name);
                    Tool.log(repository, "- code: " + newResqml2_0_1Path);

                    EA.Element fesapiResqml2_0_1Class = fesapiResqml2_0_1ClassList.Find(c => c.Name.Equals(startingFesapiClass.Name));
                    if (fesapiResqml2_0_1Class != null)
                    {
                        generateResqmlGetterSet(fesapiResqml2_0_1Class, energisticsResqml2_0_1Attribute, pathName + energisticsResqml2_0_1Attribute.Name, newResqml2_0_1Path);
                    }
                    else
                    {
                        generateResqmlGetterSet(startingFesapiClass, energisticsResqml2_0_1Attribute, pathName + energisticsResqml2_0_1Attribute.Name, newResqml2_0_1Path);
                    }
                }
            }

            // on explore les attributs côté Resqml 2.2 (pour chopper ceux qui ne sont pas communs)
            foreach (EA.Attribute energisticsResqml2_2Attribute in currentEnergisticsResqml2_2Class.Attributes)
            {
                EA.Attribute energisticsResqml2_0_1Attribute = null;
                foreach (EA.Attribute currentEnergisticsResqml2_0_1Attribute in currentEnergisticsResqml2_0_1Class.Attributes)
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
                        newResqml2_2Path = resqml2_2AttributeAccessExpression + "->" + energisticsResqml2_2Attribute.Name + "[index" + resqml2_2Index + "]";
                    }
                    // sinon si l'attribut est optionnel
                    else if (energisticsResqml2_2Attribute.LowerBound == "0")
                    {
                        newResqml2_2Path = resqml2_2AttributeAccessExpression + "->(*" + energisticsResqml2_2Attribute.Name + ")";
                    }
                    // sinon s'il est unique et mandatory
                    else
                    {
                        newResqml2_2Path = resqml2_2AttributeAccessExpression + "->" + energisticsResqml2_2Attribute.Name;
                    }

                    Tool.log(repository, "Attribute relation (resqml2_2): " + pathName + energisticsResqml2_2Attribute.Name);
                    Tool.log(repository, "- code: " + newResqml2_2Path);

                    EA.Element fesapiResqml2_2Class = fesapiResqml2_2ClassList.Find(c => c.Name.Equals(startingFesapiClass.Name));
                    if (fesapiResqml2_2Class != null)
                    {
                        generateResqmlGetterSet(fesapiResqml2_2Class, energisticsResqml2_2Attribute, pathName + energisticsResqml2_2Attribute.Name, newResqml2_2Path);
                    }
                    else
                    {
                        generateResqmlGetterSet(startingFesapiClass, energisticsResqml2_2Attribute, pathName + energisticsResqml2_2Attribute.Name, newResqml2_2Path);
                    }
                }
            }

            #endregion

            // on regarde tous les connecteurs côté Resqml 2.0.1
            foreach (EA.Connector energisticsResqml2_0_1Connector in currentEnergisticsResqml2_0_1Class.Connectors)
            {
                // on traite d'abord le cas des connecteurs hors generalisation
                if (energisticsResqml2_0_1Connector.Type != "Generalization" && energisticsResqml2_0_1Connector.ClientID == currentEnergisticsResqml2_0_1Class.ElementID)
                {
                    EA.Connector energisticsResqml2_2Connector = null;
                    foreach (EA.Connector currentEnergisticsResqml2_2Connector in currentEnergisticsResqml2_2Class.Connectors)
                    {
                        if (currentEnergisticsResqml2_2Connector.Type != "Generalization" &&
                            currentEnergisticsResqml2_2Connector.ClientID == currentEnergisticsResqml2_2Class.ElementID &&
                            repository.GetElementByID(currentEnergisticsResqml2_2Connector.SupplierID).Name == repository.GetElementByID(energisticsResqml2_0_1Connector.SupplierID).Name &&
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
                        newResqml2_0_1Path = resqml2_0_1AttributeAccessExpression + "->" + energisticsResqml2_0_1Connector.SupplierEnd.Role + "[index" + resqml2_0_1Index + "]";
                        newResqml2_0_1Index++;
                        isResqml2_0_1MultipleCardinality = true;
                    }
                    else if (energisticsResqml2_0_1Connector.SupplierEnd.Cardinality == "0..1")
                    {
                        // TODO: à tester !
                        newResqml2_0_1Path = resqml2_0_1AttributeAccessExpression + "->{" + energisticsResqml2_0_1Connector.SupplierEnd.Role + "}";
                    }
                    else
                    {
                        newResqml2_0_1Path = resqml2_0_1AttributeAccessExpression + "->" + energisticsResqml2_0_1Connector.SupplierEnd.Role;
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
                            newResqml2_2Path = resqml2_2AttributeAccessExpression + "->" + energisticsResqml2_2Connector.SupplierEnd.Role + "[index" + resqml2_2Index + "]";
                            newResqml2_2Index++;
                            isResqml2_2MultipleCardinality = true;
                        }
                        else if (energisticsResqml2_2Connector.SupplierEnd.Cardinality == "0..1")
                        {
                            // TODO: à tester !
                            newResqml2_2Path = resqml2_2AttributeAccessExpression + "->{" + energisticsResqml2_2Connector.SupplierEnd.Role + "}";
                        }
                        else
                        {
                            newResqml2_2Path = resqml2_2AttributeAccessExpression + "->" + energisticsResqml2_2Connector.SupplierEnd.Role;
                        }

                        exploreModelFromCommonClassRec(
                            startingFesapiClass,
                            startingEnergisticsResqml2_0_1Class, startingEnergisticsResqml2_2Class,
                            repository.GetElementByID(energisticsResqml2_0_1Connector.SupplierID), repository.GetElementByID(energisticsResqml2_2Connector.SupplierID),
                            markedGeneralizationSet,
                            newResqml2_0_1Index, newResqml2_2Index,
                            pathName + energisticsResqml2_0_1Connector.SupplierEnd.Role,
                            newResqml2_0_1Path, newResqml2_2Path);

                        if (isResqml2_0_1MultipleCardinality && isResqml2_2MultipleCardinality)
                        {
                            if (addResqml2Getter(startingFesapiClass, pathName + energisticsResqml2_0_1Connector.SupplierEnd.Role + "Count", "unsigned int",
                                resqml2_0_1AttributeAccessExpression + "->" + energisticsResqml2_0_1Connector.SupplierEnd.Role,
                                resqml2_2AttributeAccessExpression + "->" + energisticsResqml2_2Connector.SupplierEnd.Role,
                                "res = " + resqml2_0_1AttributeAccessExpression + "->" + energisticsResqml2_0_1Connector.SupplierEnd.Role + ".size()",
                                "res = " + resqml2_2AttributeAccessExpression + "->" + energisticsResqml2_2Connector.SupplierEnd.Role + ".size()") == null)
                            {
                                Tool.log(repository, "Unable to properly add count getter for the " + energisticsResqml2_0_1Connector.SupplierEnd.Role + " relation of the fesapi/Class Model/" + repository.GetPackageByID(startingFesapiClass.PackageID).Name + "/" + startingFesapiClass.Name + "!");
                            }
                        }
                        else if (isResqml2_0_1MultipleCardinality)
                        {
                            if (addResqml2Getter(startingFesapiClass, pathName + energisticsResqml2_0_1Connector.SupplierEnd.Role + "Count", "unsigned int",
                                resqml2_0_1AttributeAccessExpression + "->" + energisticsResqml2_0_1Connector.SupplierEnd.Role,
                                "",
                                "res = " + resqml2_0_1AttributeAccessExpression + "->" + energisticsResqml2_0_1Connector.SupplierEnd.Role + ".size()",
                                "res = 1") == null)
                            {
                                Tool.log(repository, "Unable to properly add count getter for the " + energisticsResqml2_0_1Connector.SupplierEnd.Role + " relation of the fesapi/Class Model/" + repository.GetPackageByID(startingFesapiClass.PackageID).Name + "/" + startingFesapiClass.Name + "!");
                            }
                        }
                        else if (isResqml2_2MultipleCardinality)
                        {
                            if (addResqml2Getter(startingFesapiClass, pathName + energisticsResqml2_0_1Connector.SupplierEnd.Role + "Count", "unsigned int",
                                "",
                                resqml2_2AttributeAccessExpression + "->" + energisticsResqml2_2Connector.SupplierEnd.Role,
                                "res = 1",
                                "res = " + resqml2_2AttributeAccessExpression + "->" + energisticsResqml2_2Connector.SupplierEnd.Role + ".size()") == null)
                            {
                                Tool.log(repository, "Unable to properly add count getter for the " + energisticsResqml2_0_1Connector.SupplierEnd.Role + " relation of the fesapi/Class Model/" + repository.GetPackageByID(startingFesapiClass.PackageID).Name + "/" + startingFesapiClass.Name + "!");
                            }
                        }
                    }
                    else
                    {
                        EA.Element fesapiResqml2_0_1Class = fesapiResqml2_0_1ClassList.Find(c => c.Name.Equals(startingFesapiClass.Name));
                        if (fesapiResqml2_0_1Class == null)
                        {
                            fesapiResqml2_0_1Class = startingFesapiClass;
                        }

                        exploreModelRec(
                            fesapiResqml2_0_1Class,
                            startingEnergisticsResqml2_0_1Class,
                            repository.GetElementByID(energisticsResqml2_0_1Connector.SupplierID),
                            markedGeneralizationSet,
                            newResqml2_0_1Index,
                            pathName + energisticsResqml2_0_1Connector.SupplierEnd.Role,
                            newResqml2_0_1Path);

                        if (isResqml2_0_1MultipleCardinality)
                        {
                            if (addResqmlGetter(startingFesapiClass, Tool.getGsoapProxyName(repository, startingEnergisticsResqml2_0_1Class), pathName + energisticsResqml2_0_1Connector.SupplierEnd.Role + "Count", "unsigned int",
                                resqml2_0_1AttributeAccessExpression + "->" + energisticsResqml2_0_1Connector.SupplierEnd.Role,
                                "res = " + resqml2_0_1AttributeAccessExpression + "->" + energisticsResqml2_0_1Connector.SupplierEnd.Role + ".size()") == null)
                            {
                                Tool.log(repository, "Unable to properly add count getter for the " + energisticsResqml2_0_1Connector.SupplierEnd.Role + " relation of the fesapi/Class Model/" + repository.GetPackageByID(startingFesapiClass.PackageID).Name + "/" + startingFesapiClass.Name + "!");
                            }
                        }
                    }
                }
                // on traite ensuite les relations de type generalisation
                else if (energisticsResqml2_0_1Connector.Type == "Generalization")
                {
                    EA.Connector energisticsResqml2_2Connector = null;
                    foreach (EA.Connector currentEnergisticsResqml2_2Connector in currentEnergisticsResqml2_2Class.Connectors)
                    {
                        if (currentEnergisticsResqml2_2Connector.Type == "Generalization" &&
                            (repository.GetElementByID(currentEnergisticsResqml2_2Connector.SupplierID).Name == repository.GetElementByID(energisticsResqml2_0_1Connector.SupplierID).Name || (repository.GetElementByID(currentEnergisticsResqml2_2Connector.SupplierID).Name == "AbstractObject" && repository.GetElementByID(energisticsResqml2_0_1Connector.SupplierID).Name == "AbstractResqmlDataObject")) &&
                            repository.GetElementByID(currentEnergisticsResqml2_2Connector.ClientID).Name == repository.GetElementByID(energisticsResqml2_0_1Connector.ClientID).Name)
                        {
                            energisticsResqml2_2Connector = currentEnergisticsResqml2_2Connector;
                            break;
                        }
                    }

                    if (energisticsResqml2_2Connector != null && Tool.isTopLevelClass(currentEnergisticsResqml2_0_1Class) && Tool.isTopLevelClass(currentEnergisticsResqml2_2Class))
                    {
                        continue;
                    }
                    else if (energisticsResqml2_2Connector != null && !Tool.isTopLevelClass(currentEnergisticsResqml2_0_1Class) && !Tool.isTopLevelClass(currentEnergisticsResqml2_2Class))
                    {
                        // par construction, si ce n'est pas marké côté Resqml 2.0.1, ce n'est pas non pluc marqué côté Resqml 2.2
                        // Attention toutefois à bien marquer les 2 dans le if dans le cas ou on appelerai récursivement la version non Resqml2
                        //if (!markedGeneralizationSet.Contains(energisticsResqml2_0_1Connector.ConnectorID.ToString()))
                        if (!markedGeneralizationSet.Contains(energisticsResqml2_0_1Connector.ConnectorID))
                        {
                            if (energisticsResqml2_0_1Connector.ClientID == currentEnergisticsResqml2_0_1Class.ElementID)
                            {
                                markedGeneralizationSet.Add(energisticsResqml2_0_1Connector.ConnectorID);
                                markedGeneralizationSet.Add(energisticsResqml2_2Connector.SupplierID);
                                exploreModelFromCommonClassRec(
                                    startingFesapiClass,
                                    startingEnergisticsResqml2_0_1Class, startingEnergisticsResqml2_2Class,
                                    repository.GetElementByID(energisticsResqml2_0_1Connector.SupplierID), repository.GetElementByID(energisticsResqml2_2Connector.SupplierID),
                                    //markedGeneralizationSet + (energisticsResqml2_0_1Connector.ConnectorID) + "." + (energisticsResqml2_2Connector.ConnectorID) + ".",
                                    markedGeneralizationSet,
                                    resqml2_0_1Index, resqml2_2Index,
                                    pathName,
                                    resqml2_0_1AttributeAccessExpression, resqml2_2AttributeAccessExpression);
                            }
                            else
                            {
                                markedGeneralizationSet.Add(energisticsResqml2_0_1Connector.ConnectorID);
                                markedGeneralizationSet.Add(energisticsResqml2_2Connector.ConnectorID);
                                exploreModelFromCommonClassRec(
                                    startingFesapiClass,
                                    startingEnergisticsResqml2_0_1Class, startingEnergisticsResqml2_2Class,
                                    repository.GetElementByID(energisticsResqml2_0_1Connector.ClientID), repository.GetElementByID(energisticsResqml2_2Connector.ClientID),
                                    //markedGeneralizationSet + (energisticsResqml2_0_1Connector.ConnectorID) + "." + (energisticsResqml2_2Connector.ConnectorID) + ".",
                                    markedGeneralizationSet,
                                    resqml2_0_1Index, resqml2_2Index,
                                    pathName + repository.GetElementByID(energisticsResqml2_0_1Connector.ClientID).Name,
                                    "static_cast<" + Tool.getGsoapName(repository, repository.GetElementByID(energisticsResqml2_0_1Connector.ClientID)) + "*>(" + resqml2_0_1AttributeAccessExpression + ")",
                                    "static_cast<" + Tool.getGsoapName(repository, repository.GetElementByID(energisticsResqml2_2Connector.ClientID)) + "*>(" + resqml2_2AttributeAccessExpression + ")");
                            }
                        }
                    }
                    else if (energisticsResqml2_2Connector != null && Tool.isTopLevelClass(currentEnergisticsResqml2_0_1Class))
                    {
                        //if (!markedGeneralizationSet.Contains(energisticsResqml2_2Connector.ConnectorID.ToString()))
                        if (!markedGeneralizationSet.Contains(energisticsResqml2_2Connector.ConnectorID))
                        {
                            EA.Element fesapiResqml2_2Class = fesapiResqml2_2ClassList.Find(c => c.Name.Equals(startingFesapiClass.Name));
                            if (fesapiResqml2_2Class == null)
                            {
                                fesapiResqml2_2Class = startingFesapiClass;
                            }

                            if (energisticsResqml2_2Connector.ClientID == currentEnergisticsResqml2_2Class.ElementID)
                            {
                                markedGeneralizationSet.Add(energisticsResqml2_2Connector.ConnectorID);
                                exploreModelRec(
                                   fesapiResqml2_2Class,
                                   startingEnergisticsResqml2_2Class,
                                   repository.GetElementByID(energisticsResqml2_2Connector.SupplierID),
                                   //markedGeneralizationSet + (energisticsResqml2_2Connector.ConnectorID) + ".",
                                   markedGeneralizationSet,
                                   resqml2_2Index,
                                   pathName,
                                   resqml2_2AttributeAccessExpression);
                            }
                            else
                            {
                                markedGeneralizationSet.Add(energisticsResqml2_2Connector.ConnectorID);
                                exploreModelRec(
                                    fesapiResqml2_2Class,
                                    startingEnergisticsResqml2_2Class,
                                    repository.GetElementByID(energisticsResqml2_2Connector.ClientID),
                                    //markedGeneralizationSet + (energisticsResqml2_2Connector.ConnectorID) + ".",
                                    markedGeneralizationSet,
                                    resqml2_2Index,
                                    pathName + repository.GetElementByID(energisticsResqml2_2Connector.ClientID).Name,
                                    "static_cast<" + Tool.getGsoapName(repository, repository.GetElementByID(energisticsResqml2_2Connector.ClientID)) + "*>(" + resqml2_2AttributeAccessExpression + ")");
                            }
                        }
                    }
                    else if (!Tool.isTopLevelClass(currentEnergisticsResqml2_0_1Class))
                    {
                        // par construction, si ce n'est pas marké côté Resqml 2.0.1, ce n'est pas non pluc marqué côté Resqml 2.2
                        // Attention toutefois à bien marquer les 2 dans le if dans le cas ou on appelerai récursivement la version non Resqml2
                        //if (!markedGeneralizationSet.Contains(energisticsResqml2_0_1Connector.ConnectorID.ToString()))
                        if (!markedGeneralizationSet.Contains(energisticsResqml2_0_1Connector.ConnectorID))
                        {
                            EA.Element fesapiResqml2_0_1Class = fesapiResqml2_0_1ClassList.Find(c => c.Name.Equals(startingFesapiClass.Name));
                            if (fesapiResqml2_0_1Class == null)
                            {
                                fesapiResqml2_0_1Class = startingFesapiClass;
                            }

                            if (energisticsResqml2_0_1Connector.ClientID == currentEnergisticsResqml2_0_1Class.ElementID)
                            {
                                markedGeneralizationSet.Add(energisticsResqml2_0_1Connector.ConnectorID);
                                exploreModelRec(
                                    fesapiResqml2_0_1Class,
                                    startingEnergisticsResqml2_0_1Class,
                                    repository.GetElementByID(energisticsResqml2_0_1Connector.SupplierID),
                                    //markedGeneralizationSet + (energisticsResqml2_0_1Connector.ConnectorID) + ".",
                                    markedGeneralizationSet,
                                    resqml2_0_1Index,
                                    pathName,
                                    resqml2_0_1AttributeAccessExpression);
                            }
                            else
                            {
                                markedGeneralizationSet.Add(energisticsResqml2_0_1Connector.ConnectorID);
                                exploreModelRec(
                                    fesapiResqml2_0_1Class,
                                    startingEnergisticsResqml2_0_1Class,
                                    repository.GetElementByID(energisticsResqml2_0_1Connector.ClientID),
                                    //markedGeneralizationSet + (energisticsResqml2_0_1Connector.ConnectorID) + ".",
                                    markedGeneralizationSet,
                                    resqml2_0_1Index,
                                    pathName + repository.GetElementByID(energisticsResqml2_0_1Connector.ClientID).Name,
                                    "static_cast<" + Tool.getGsoapName(repository, repository.GetElementByID(energisticsResqml2_0_1Connector.ClientID)) + "*>(" + resqml2_0_1AttributeAccessExpression + ")");
                            }
                        }
                    }
                }
            }

            // on regarde ensuite les relations côté Resqml 2.2
            foreach (EA.Connector energisticsResqml2_2Connector in currentEnergisticsResqml2_2Class.Connectors)
            {
                // on traite d'abord le cas des connecteurs hors generalisation
                if (energisticsResqml2_2Connector.Type != "Generalization" && energisticsResqml2_2Connector.ClientID == currentEnergisticsResqml2_2Class.ElementID)
                {
                    //Tool.log(repository, "DEBUG: " + energisticsResqml2_0_1Class.Name + "-" + energisticsResqml2_2Connector.SupplierEnd.Role + "->" + repository.GetElementByID(energisticsResqml2_2Connector.SupplierID).Name);

                    EA.Connector energisticsResqml2_0_1Connector = null;
                    foreach (EA.Connector currentEnergisticsResqml2_0_1Connector in currentEnergisticsResqml2_0_1Class.Connectors)
                    {
                        if (currentEnergisticsResqml2_0_1Connector.Type != "Generalization" &&
                            currentEnergisticsResqml2_0_1Connector.ClientID == currentEnergisticsResqml2_0_1Class.ElementID &&
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
                            newResqml2_2Path = resqml2_2AttributeAccessExpression + "->" + energisticsResqml2_2Connector.SupplierEnd.Role + "[index" + resqml2_2Index + "]";
                            newResqml2_2Index++;
                            isResqml2_2MultipleCardinality = true;
                        }
                        else if (energisticsResqml2_2Connector.SupplierEnd.Cardinality == "0..1")
                        {
                            // TODO: à tester
                            newResqml2_2Path = resqml2_2AttributeAccessExpression + "->{" + energisticsResqml2_2Connector.SupplierEnd.Role + "}";
                        }
                        else
                        {
                            newResqml2_2Path = resqml2_2AttributeAccessExpression + "->" + energisticsResqml2_2Connector.SupplierEnd.Role;
                        }

                        EA.Element fesapiResqml2_2Class = fesapiResqml2_2ClassList.Find(c => c.Name.Equals(startingFesapiClass.Name));
                        if (fesapiResqml2_2Class == null)
                        {
                            fesapiResqml2_2Class = startingFesapiClass;
                        }

                        exploreModelRec(
                            fesapiResqml2_2Class,
                            startingEnergisticsResqml2_2Class,
                            repository.GetElementByID(energisticsResqml2_2Connector.SupplierID),
                            markedGeneralizationSet,
                            newResqml2_2Index,
                            pathName + energisticsResqml2_2Connector.SupplierEnd.Role,
                            newResqml2_2Path);

                        if (isResqml2_2MultipleCardinality)
                        {
                            if (addResqmlGetter(startingFesapiClass, Tool.getGsoapProxyName(repository, startingEnergisticsResqml2_2Class), pathName + energisticsResqml2_2Connector.SupplierEnd.Role + "Count", "unsigned int",
                                resqml2_2AttributeAccessExpression + "->" + energisticsResqml2_2Connector.SupplierEnd.Role,
                                "res = " + resqml2_2AttributeAccessExpression + "->" + energisticsResqml2_2Connector.SupplierEnd.Role + ".size()") == null)
                            {
                                Tool.log(repository, "Unable to properly add count getter for the " + energisticsResqml2_2Connector.SupplierEnd.Role + " relation of the fesapi/Class Model/" + repository.GetPackageByID(startingFesapiClass.PackageID).Name + "/" + startingFesapiClass.Name + "!");
                            }
                        }
                    }
                }
                else if (energisticsResqml2_2Connector.Type == "Generalization")
                {
                    EA.Connector energisticsResqml2_0_1Connector = null;
                    foreach (EA.Connector currentEnergisticsResqml2_0_1Connector in currentEnergisticsResqml2_0_1Class.Connectors)
                    {
                        if (currentEnergisticsResqml2_0_1Connector.Type == "Generalization" &&
                            (repository.GetElementByID(currentEnergisticsResqml2_0_1Connector.SupplierID).Name == repository.GetElementByID(energisticsResqml2_2Connector.SupplierID).Name || (repository.GetElementByID(currentEnergisticsResqml2_0_1Connector.SupplierID).Name == "AbstractObject" && repository.GetElementByID(energisticsResqml2_2Connector.SupplierID).Name == "AbstractResqmlDataObject")) &&
                            repository.GetElementByID(currentEnergisticsResqml2_0_1Connector.ClientID).Name == repository.GetElementByID(energisticsResqml2_2Connector.ClientID).Name)
                        {
                            energisticsResqml2_0_1Connector = currentEnergisticsResqml2_0_1Connector;
                            break;
                        }
                    }

                    if (energisticsResqml2_0_1Connector == null && !Tool.isTopLevelClass(currentEnergisticsResqml2_2Class))
                    {
                        //if (!markedGeneralizationSet.Contains(energisticsResqml2_2Connector.ConnectorID.ToString()))
                        if (!markedGeneralizationSet.Contains(energisticsResqml2_2Connector.ConnectorID))
                        {
                            EA.Element fesapiResqml2_2Class = fesapiResqml2_2ClassList.Find(c => c.Name.Equals(startingFesapiClass.Name));
                            if (fesapiResqml2_2Class == null)
                            {
                                fesapiResqml2_2Class = startingFesapiClass;
                            }

                            if (energisticsResqml2_2Connector.ClientID == currentEnergisticsResqml2_2Class.ElementID)
                            {
                                markedGeneralizationSet.Add(energisticsResqml2_2Connector.ConnectorID);
                                exploreModelRec(
                                    fesapiResqml2_2Class,
                                    startingEnergisticsResqml2_2Class,
                                    repository.GetElementByID(energisticsResqml2_2Connector.SupplierID),
                                    //markedGeneralizationSet + (energisticsResqml2_2Connector.ConnectorID) + ".",
                                    markedGeneralizationSet,
                                    resqml2_2Index,
                                    pathName,
                                    resqml2_2AttributeAccessExpression);
                            }
                            else
                            {
                                markedGeneralizationSet.Add(energisticsResqml2_2Connector.ConnectorID);
                                exploreModelRec(
                                    fesapiResqml2_2Class,
                                    startingEnergisticsResqml2_2Class,
                                    repository.GetElementByID(energisticsResqml2_2Connector.ClientID),
                                    //markedGeneralizationSet + (energisticsResqml2_2Connector.ConnectorID) + ".",
                                    markedGeneralizationSet,
                                    resqml2_2Index,
                                    pathName + repository.GetElementByID(energisticsResqml2_2Connector.ClientID).Name,
                                    "static_cast<" + Tool.getGsoapName(repository, repository.GetElementByID(energisticsResqml2_2Connector.ClientID)) + "*>(" + resqml2_2AttributeAccessExpression + ")");
                            }
                        }
                    }
                }
            }
        }

        private void exploreModel(EA.Element fesapiClass, EA.Element energisticsClass)
        {
            Tool.log(repository, "========================");
            Tool.log(repository, "begin exploreRelationSetBis...");

            Tool.log(repository, "class: " + energisticsClass.Name);
            SortedSet<int> markedGeneralizationSet = new SortedSet<int>();
            exploreModelRec(
                fesapiClass, 
                energisticsClass, 
                energisticsClass, 
                //"", 
                markedGeneralizationSet,
                0, 
                "",
                "static_cast<" + Tool.getGsoapName(repository, energisticsClass) + "*>(" + Tool.getGsoapProxyName(repository, energisticsClass) + ")");

            Tool.log(repository, "... end exploreRelationSetBis");
            Tool.log(repository, "========================");
        }

        private void exploreModelRec(
            EA.Element startingFesapiClass,
            EA.Element startingEnergisticsClass,
            EA.Element currentEnergisticsClass,
            //string markedGeneralizationSet,
            SortedSet<int> markedGeneralizationSet,
            uint index,
            string pathName,
            string attributeAccessExpression)
        {
            // je le chemin me mène à un top level je m'arrête
            if ((Tool.isTopLevelClass(currentEnergisticsClass) || currentEnergisticsClass.Name.Equals("AbstractResqmlDataObject") || currentEnergisticsClass.Name.Equals("AbstractObject")) && attributeAccessExpression != "static_cast<" + Tool.getGsoapName(repository, startingEnergisticsClass) + "*>(" + Tool.getGsoapProxyName(repository, startingEnergisticsClass) + ")") //energisticsClass != startingEnergisticsClass) // path != "gsoapProxy")
            {
                Tool.log(repository, "TopLevelObject relation (" + Tool.getFesapiNamespace(repository, currentEnergisticsClass) + "): " + attributeAccessExpression);

                string fesapiNamespace = Tool.getFesapiNamespace(repository, currentEnergisticsClass);

                EA.Element fesapiDestClass;

                if (fesapiNamespace == "resqml2_0_1")
                {
                    fesapiDestClass = fesapiResqml2ClassList.Find(c => c.Name == currentEnergisticsClass.Name);

                    if (fesapiDestClass == null)
                    {
                        fesapiDestClass = fesapiResqml2_0_1ClassList.Find(c => c.Name == currentEnergisticsClass.Name);
                    }
                }
                else // if (fesapiNamespace == "resqml2_2")
                {
                    fesapiDestClass = fesapiResqml2ClassList.Find(c => c.Name == currentEnergisticsClass.Name);

                    if (fesapiDestClass == null)
                    {
                        fesapiDestClass = fesapiResqml2_2ClassList.Find(c => c.Name == currentEnergisticsClass.Name);
                    }
                }

                if (fesapiDestClass != null)
                {
                    if (Tool.getGsoapProxyName(repository, startingEnergisticsClass) == Constants.gsoapProxy2_0_1)
                    {
                        generateTopLevelRelation(startingFesapiClass, fesapiDestClass, startingEnergisticsClass, null, attributeAccessExpression, "", pathName);
                    }
                    else
                    {
                        generateTopLevelRelation(startingFesapiClass, fesapiDestClass, null, startingEnergisticsClass, "", attributeAccessExpression, pathName);
                    }

                }
                else
                {
                    Tool.log(repository, "Not able to handle " + startingFesapiClass.Name + " to " + currentEnergisticsClass.Name + " relation. Destination class is missing from fesapi!");
                }


                //string packageName = repository.GetPackageByID(fesapiClass.PackageID).Name;
                //EA.Element fesapiDestClass;
                //if (packageName == "resqml2")
                //{
                //    fesapiDestClass = fesapiResqml2ClassList.Find(c => c.Name == energisticsClass.Name);
                //}
                //else if (packageName == "resqml2_0_1")
                //{
                //    fesapiDestClass = fesapiResqml2_0_1ClassList.Find(c => c.Name == energisticsClass.Name);
                //}
                //else // packageName == "resqml2_2"
                //{
                //    fesapiDestClass = fesapiResqml2_2ClassList.Find(c => c.Name == energisticsClass.Name);
                //}

                //if (fesapiDestClass != null)
                //{
                //    addResqmlTopLevel2TopLevelRelation(fesapiClass, fesapiDestClass);
                //}
                //else
                //{
                //    Tool.log(repository, "Not able to handle " + fesapiClass.Name + " to " + energisticsClass.Name + " relation. Destination class is missing from fesapi/" + packageName + "!");
                //}
                return;
            }

            // j'explore tout les attributs de la classe courante
            foreach (EA.Attribute attribute in currentEnergisticsClass.Attributes)
            {
                string newPath;
                // si la multiplicité est supérieur à 1
                if (!(attribute.UpperBound == "1"))
                {
                    Tool.log(repository, "Attribute relation (" + Tool.getFesapiNamespace(repository, currentEnergisticsClass) + "): " + pathName + attribute.Name);
                    Tool.log(repository, "-code: " + attributeAccessExpression + "->" + attribute.Name + "[index" + index + "]");
                    newPath = attributeAccessExpression + "->" + attribute.Name + "[index" + index + "]";
                }
                // sinon si l'attribut est optionnel
                else if (attribute.LowerBound == "0")
                {
                    Tool.log(repository, "Attribute relation (" + Tool.getFesapiNamespace(repository, currentEnergisticsClass) + "): " + pathName + attribute.Name);
                    Tool.log(repository, "- code: " + attributeAccessExpression + "->(*" + attribute.Name + ")");
                    newPath = attributeAccessExpression + "->(*" + attribute.Name + ")";
                }
                // sinon s'il est unique et mandatory
                else
                {
                    Tool.log(repository, "Attribute relation (" + Tool.getFesapiNamespace(repository, currentEnergisticsClass) + "): " + pathName + attribute.Name);
                    Tool.log(repository, "- code: " + attributeAccessExpression + "->" + attribute.Name);
                    newPath = attributeAccessExpression + "->" + attribute.Name;
                }

                generateResqmlGetterSet(startingFesapiClass, attribute, pathName + attribute.Name, newPath);
            }

            foreach (EA.Connector connector in currentEnergisticsClass.Connectors)
            {
                if (connector.Type != "Generalization" && connector.ClientID == currentEnergisticsClass.ElementID)
                {
                    string newPath;
                    uint newIndex = index;
                    bool isMultipleCardinality = false;

                    if (connector.SupplierEnd.Cardinality == "*" || connector.SupplierEnd.Cardinality == "0..*" || connector.SupplierEnd.Cardinality == "1..*")
                    {
                        newPath = attributeAccessExpression + "->" + connector.SupplierEnd.Role + "[index" + index + "]";
                        newIndex++;
                        isMultipleCardinality = true;
                    }
                    else if (connector.SupplierEnd.Cardinality == "0..1")
                    {
                        // TODO: à tester !
                        newPath = attributeAccessExpression + "->{" + connector.SupplierEnd.Role + "}";
                    }
                    else
                    {
                        newPath = attributeAccessExpression + "->" + connector.SupplierEnd.Role;
                    }

                    exploreModelRec(
                        startingFesapiClass,
                        startingEnergisticsClass,
                        repository.GetElementByID(connector.SupplierID),
                        markedGeneralizationSet,
                        newIndex,
                        pathName + connector.SupplierEnd.Role,
                        newPath);

                    if (isMultipleCardinality)
                    {
                        if (addResqmlGetter(startingFesapiClass, Tool.getGsoapProxyName(repository, startingEnergisticsClass), pathName + connector.SupplierEnd.Role + "Count", "unsigned int",
                            attributeAccessExpression + "->" + connector.SupplierEnd.Role,
                            "res = " + attributeAccessExpression + "->" + connector.SupplierEnd.Role + ".size()") == null)
                        {
                            Tool.log(repository, "Unable to properly add count getter for the " + connector.SupplierEnd.Role + " relation of the fesapi/Class Model/" + repository.GetPackageByID(startingFesapiClass.PackageID).Name + "/" + startingFesapiClass.Name + "!");
                        }
                    }
                }
                else if (connector.Type == "Generalization")
                {
                    if (Tool.isTopLevelClass(currentEnergisticsClass))
                    {
                        continue;
                    }

                    //if (!markedGeneralizationSet.Contains(connector.ConnectorID.ToString()))
                    if (!markedGeneralizationSet.Contains(connector.ConnectorID))
                    {
                        if (connector.ClientID == currentEnergisticsClass.ElementID)
                        {
                            markedGeneralizationSet.Add(connector.ConnectorID);
                            exploreModelRec(
                                startingFesapiClass,
                                startingEnergisticsClass,
                                repository.GetElementByID(connector.SupplierID),
                                //markedGeneralizationSet + (connector.ConnectorID) + ".",
                                markedGeneralizationSet,
                                index,
                                pathName,
                                attributeAccessExpression);
                        }
                        else
                        {
                            markedGeneralizationSet.Add(connector.ConnectorID);
                            exploreModelRec(
                                startingFesapiClass,
                                startingEnergisticsClass,
                                repository.GetElementByID(connector.ClientID),
                                //markedGeneralizationSet + (connector.ConnectorID) + ".",
                                markedGeneralizationSet,
                                index,
                                pathName + repository.GetElementByID(connector.ClientID).Name,
                                "static_cast<" + Tool.getGsoapName(repository, repository.GetElementByID(connector.ClientID)) + "*>(" + attributeAccessExpression + ")");
                        }
                    }
                }
            }
        }

        #region inheritance

        private void generateInheritance()
        {
            // *******************************
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

            // ***********************************
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
                Tool.log(repository, "Base class of fesapi/Class Model/resqml2_0_1/" + fesapiResqml2_0_1Class.Name + " does not exist in fesapi/Class Model. No generalization connector will be generated!");
            }

            // *********************************
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
                Tool.log(repository, "Base class of fesapi/Class Model/resqml2_2/" + fesapiResqml2_2Class.Name + " does not exist in fesapi/Class Model. No generalization connector will be generated!");
            }
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

            addOrUpdateFesapiIncludeTag(childClass, "#include \"" + includeTagValue + "\";");

            //EA.TaggedValue fesapiBaseClassIncludeTag = childClass.TaggedValues.GetByName(Constants.fesapiIncludeTag);

            //if (fesapiBaseClassIncludeTag == null)
            //{
            //    fesapiBaseClassIncludeTag = childClass.TaggedValues.AddNew(Constants.fesapiIncludeTag, "");
            //}

            //fesapiBaseClassIncludeTag.Value += "#include \"" + includeTagValue + "\";";
            //if (!fesapiBaseClassIncludeTag.Update())
            //{
            //    Tool.showMessageBox(repository, fesapiBaseClassIncludeTag.GetLastError());
            //}

            //EA.TaggedValue fesapiIncludeTag = childClass.TaggedValues.AddNew(Constants.fesapiBaseClassIncludeTagName, includeTagValue);
            //if (!(fesapiIncludeTag.Update()))
            //{
            //    Tool.log(repository, fesapiIncludeTag.GetLastError());
            //    return null;
            //}

            // tagging the child class with the fesapi base class namespace (that is to say the name of its parent package)
            EA.TaggedValue fesapiBaseClassNamespaceTag = childClass.TaggedValues.AddNew(Constants.fesapiBaseClassNamespaceTagName, baseClassPackageName);
            if (!(fesapiBaseClassNamespaceTag.Update()))
            {
                Tool.log(repository, fesapiBaseClassNamespaceTag.GetLastError());
                return null;
            }

            childClass.TaggedValues.Refresh();

            return generalizationConnector;
        }

        #endregion

        #region constructors/destructors

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

        #region getters
        
        string generateAttributeAccessCode(string attributeAccessExpression, string returnExpression)
        {
            return generateAttributeAccessCodeRec(attributeAccessExpression, returnExpression, 1, 1, "\t");
        }

        string generateAttributeAccessCodeRec(string attributeAccessExpression, string returnExpression, int indexOccurence, int pointerOccurence, string tabulation)
        {
            // on récupère la position du indexOccurence-ième "[index"
            int indexPos = Tool.indexOfOccurence(attributeAccessExpression, "[index", indexOccurence);

            // on récupère la position du pointerOccurence-ième "{"
            int pointerPos = Tool.indexOfOccurence(attributeAccessExpression, "{", pointerOccurence);

            if (indexPos == -1 && pointerPos == -1)
            {
                return tabulation + returnExpression.Replace("{", "").Replace("}", "") + ";\n";
            }
            else if (indexPos == -1 || (pointerPos != -1 && pointerPos<indexPos))
            {
                // on garde ce qui précède le "{"
                string prefix = attributeAccessExpression.Substring(0, pointerPos);

                // on garde ce qui suit le "{"
                string suffix = attributeAccessExpression.Substring(pointerPos + 1);

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
                res += generateAttributeAccessCodeRec(attributeAccessExpression, returnExpression, indexOccurence, pointerOccurence + 1, tabulation + "\t");
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
                string prefix = attributeAccessExpression.Substring(0, indexPos);

                // on mémorise ce qui suit le "[index"
                string suffix = attributeAccessExpression.Substring(indexPos + 6);

                // on y compte le nombre de parenthèses fermantes moins le nombre de parenthèse ouvrante
                int bracketsCount = Tool.countStringOccurrences(suffix, ')'.ToString()) - Tool.countStringOccurrences(suffix, '('.ToString());

                // on calcule la position de la bracketCount-ieme parenthèse ouvrante
                int bracketPos = Tool.getNthIndex(prefix, '(', bracketsCount);

                // je retire de res tout ce qui précède (inclue) la bracketCount-ieme parenthèse ouvrante
                prefix = prefix.Remove(0, bracketPos + 1);

                string res = "";
                res += tabulation + "if (index" + (indexOccurence - 1) + " < " + prefix + ".size())\n";
                res += tabulation + "{\n";
                res += generateAttributeAccessCodeRec(attributeAccessExpression, returnExpression, indexOccurence + 1, pointerOccurence, tabulation + "\t");
                res += tabulation + "}\n";
                res += tabulation + "else\n";
                res += tabulation + "{\n";
                res += tabulation + "\tthrow out_of_range(\"index" + (indexOccurence - 1) + " is out of range\");\n";
                res += tabulation + "}\n";

                return res;
            }
        }
        
        void generateResqmlGetterSet(EA.Element fesapiClass, EA.Attribute energisticsAttribute, string unprefixedGetterName, string attributeAccessExpression)
        {
            // get the fesapiClass package name for explciting log messages
            string packageName = repository.GetPackageByID(fesapiClass.PackageID).Name;

            string gsoapProxy = Tool.getGsoapProxyName(repository, repository.GetElementByID(energisticsAttribute.ParentID));

            string getterName = "get" + Tool.upperCaseFirstLetter(unprefixedGetterName);

            // if the type of the attribute is a basic type
            string attributeBasicType = Tool.getBasicType(repository, energisticsAttribute);
            if (!(attributeBasicType.Equals("")))
            {
                if (addResqmlGetter(fesapiClass, gsoapProxy, getterName, attributeBasicType, attributeAccessExpression, "res = " + attributeAccessExpression) == null)
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
                    if (addResqmlGetter(fesapiClass, gsoapProxy, getterName, "double", attributeAccessExpression, "res = " + attributeAccessExpression + "->__item") == null)
                    {
                        Tool.log(repository, "Unable to properly add measure value getter for the " + energisticsAttribute.Name + " attribute of the fesapi/Class Model/" + packageName + "/" + fesapiClass.Name + "!");
                        return;
                    }

                    // TODO: à tester
                    // in order to generate unit of measure and string unit of measure getters 
                    generateResqmlGetterSet(fesapiClass, attributeType.Attributes.GetAt(0), getterName + "Uom", attributeAccessExpression + "->uom");
                }
                // else if the type of the attribute is an enum type
                else if (Tool.isEnum(attributeType))
                {
                    if (addResqmlGetter(fesapiClass, gsoapProxy, getterName, Tool.getGsoapName(repository, attributeType), attributeAccessExpression, "res = " + attributeAccessExpression) == null)
                    {
                        Tool.log(repository, "Unable to properly add enum value getter for the " + energisticsAttribute.Name + " attribute of the fesapi/Class Model/" + packageName + "/" + fesapiClass.Name + "!");
                        return;
                    }
                    if (addResqmlGetter(fesapiClass, gsoapProxy, getterName + "AsString", "std::string", attributeAccessExpression, "res = " + Tool.getGsoapEnum2SConverterName(repository, attributeType) + "(" + gsoapProxy + "->soap, " + attributeAccessExpression + ")") == null)
                    {
                        Tool.log(repository, "Unable to properly add string enum value getter for the " + energisticsAttribute.Name + " attribute of the fesapi/Class Model/" + packageName + "/" + fesapiClass.Name + "!");
                        return;
                    }
                }
                // else if the type of the attribute is an extended enum type
                else if (attributeType.Name.EndsWith("Ext"))
                {
                    EA.Element enumType = Tool.getEnumTypeFromEnumExtType(repository, attributeType);

                    if (addResqmlGetter(fesapiClass, gsoapProxy, getterName, Tool.getGsoapName(repository, enumType), attributeAccessExpression, Tool.getGsoapS2EnumConverterName(repository, enumType) + "(" + gsoapProxy + "->soap, " + attributeAccessExpression + ".c_str(), &res)") == null)
                    {
                        Tool.log(repository, "Unable to properly add enum value getter for the " + energisticsAttribute.Name + " attribute of the fesapi/Class Model/" + packageName + "/" + fesapiClass.Name + "!");
                        return;
                    }
                    if (addResqmlGetter(fesapiClass, gsoapProxy, getterName + "AsString", "std::string", attributeAccessExpression, "res = " + attributeAccessExpression) == null)
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
                    attributeAccessExpression.Substring(0, attributeAccessExpression.LastIndexOf("[")),
                    "res = " + attributeAccessExpression.Substring(0, attributeAccessExpression.LastIndexOf("[")) + ".size()");
                {
                    Tool.log(repository, "Unable to properly add count getter for the " + energisticsAttribute.Name + " attribute of the fesapi/Class Model/" + packageName + "/" + fesapiClass.Name + "!");
                }
            }
        }

        EA.Method addResqmlGetter(EA.Element fesapiClass, string gsoapProxy, string getterName, string returnType, string attributeAccessExpression, string returnExpression)
        {
            EA.Method getter = fesapiClass.Methods.AddNew(getterName, returnType);

            getter.Code = returnType + " res;\n";
            getter.Code += "if (" + gsoapProxy + " != nullptr)\n";
            getter.Code += "{\n";
            getter.Code += generateAttributeAccessCode(attributeAccessExpression, returnExpression);
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

            int resqml2_0_1IndexCount = Tool.countStringOccurrences(attributeAccessExpression, "[index");      
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

        private void generateResqml2GetterSet(
            EA.Element fesapiResqml2Class, 
            EA.Attribute energisticsResqml2_0_1Attribute, 
            EA.Attribute energisticsResqml2_2Attribute, 
            string unprefixedGetterName, 
            string resqml2_0_1AttributeAccessExpression, 
            string resqml2_2AttributeAccessExpression)
        {
            // get the fesapiClass package name for expliciting log messages
            string packageName = repository.GetPackageByID(fesapiResqml2Class.PackageID).Name;

            string gsoapResqml2_0_1Proxy = Tool.getGsoapProxyName(repository, repository.GetElementByID(energisticsResqml2_0_1Attribute.ParentID));
            string gsoapResqml2_2Proxy = Tool.getGsoapProxyName(repository, repository.GetElementByID(energisticsResqml2_2Attribute.ParentID));

            string getterName = "get" + Tool.upperCaseFirstLetter(unprefixedGetterName);

            string resqml2_0_1AttributeBasicType = Tool.getBasicType(repository, energisticsResqml2_0_1Attribute);
            string resqml2_2AttributeBasicType = Tool.getBasicType(repository, energisticsResqml2_2Attribute);
            // if Resqml 2.0.1 and Resqml 2.2 attributes type are basic type 
            if ((resqml2_0_1AttributeBasicType != "") && (resqml2_2AttributeBasicType != ""))
            {
                // if basic types are the same
                if (resqml2_0_1AttributeBasicType.Equals(resqml2_2AttributeBasicType))
                {
                    if (addResqml2Getter(fesapiResqml2Class, getterName, resqml2_0_1AttributeBasicType,
                        resqml2_0_1AttributeAccessExpression,
                        resqml2_2AttributeAccessExpression,
                        "res = " + resqml2_0_1AttributeAccessExpression,
                        "res = " + resqml2_2AttributeAccessExpression) == null)
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
                    if (addResqml2Getter(fesapiResqml2Class, getterName, "double", resqml2_0_1AttributeAccessExpression, resqml2_2AttributeAccessExpression, "res = " + resqml2_0_1AttributeAccessExpression + "->__item", "res = " + resqml2_2AttributeAccessExpression + "->__item") == null)
                    {
                        Tool.log(repository, "Unable to properly add measure value getter for the " + energisticsResqml2_0_1Attribute.Name + " attribute of the fesapi/Class Model/" + packageName + "/" + fesapiResqml2Class.Name + "!");
                        return;
                    }

                    // in order to generate unit of measure and string unit of measure getters
                    EA.Attribute resqml2_0_1UomAttribute = resqml2_0_1AttributeType.Attributes.GetAt(0);
                    EA.Attribute resqml2_2UomAttribute = resqml2_2AttributeType.Attributes.GetAt(0);
                    generateResqml2GetterSet(fesapiResqml2Class, resqml2_0_1UomAttribute, resqml2_2UomAttribute, getterName + "Uom", resqml2_0_1AttributeAccessExpression + "->uom", resqml2_2AttributeAccessExpression + "->uom");
                    
                }
                // else if both attributes are enum type
                else if (Tool.isEnum(resqml2_0_1AttributeType) && Tool.isEnum(resqml2_2AttributeType))
                {
                    if (addResqml2Getter(fesapiResqml2Class, getterName, Tool.getGsoapName(repository, resqml2_2AttributeType), 
                        resqml2_0_1AttributeAccessExpression, 
                        resqml2_2AttributeAccessExpression,
                        Tool.getGsoapS2EnumConverterName(repository, resqml2_2AttributeType) + "(" + gsoapResqml2_0_1Proxy + "->soap, " + Tool.getGsoapEnum2SConverterName(repository, resqml2_0_1AttributeType) + "(" + gsoapResqml2_0_1Proxy + "->soap, " + resqml2_0_1AttributeAccessExpression + "), &res)",
                        "res = " + resqml2_2AttributeAccessExpression
                        ) == null)
                    {
                        Tool.log(repository, "Unable to properly add enum value getter for the " + energisticsResqml2_0_1Attribute.Name + " attribute of the fesapi/Class Model/" + packageName + "/" + fesapiResqml2Class.Name + "!");
                        return;
                    }
                    if (addResqml2Getter(fesapiResqml2Class, getterName + "AsString", "std::string",
                        resqml2_0_1AttributeAccessExpression,
                        resqml2_2AttributeAccessExpression,
                        "res = " + Tool.getGsoapEnum2SConverterName(repository, resqml2_0_1AttributeType) + "(" + gsoapResqml2_0_1Proxy + "->soap, " + resqml2_0_1AttributeAccessExpression + ")",
                        "res = " + Tool.getGsoapEnum2SConverterName(repository, resqml2_2AttributeType) + "(" + gsoapResqml2_2Proxy + "->soap, " + resqml2_2AttributeAccessExpression + ")") == null)
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
                        resqml2_0_1AttributeAccessExpression,
                        resqml2_2AttributeAccessExpression,
                        Tool.getGsoapS2EnumConverterName(repository, enumType) + "(" + gsoapResqml2_0_1Proxy + "->soap, " + Tool.getGsoapEnum2SConverterName(repository, resqml2_0_1AttributeType) + "(" + gsoapResqml2_0_1Proxy + "->soap, " + resqml2_0_1AttributeAccessExpression + "), &res)",
                        Tool.getGsoapS2EnumConverterName(repository, enumType) + "(" + gsoapResqml2_2Proxy + "->soap, " + resqml2_2AttributeAccessExpression + ".c_str(), &res)") == null)
                    {
                        Tool.log(repository, "Unable to properly add enum value getter for the " + energisticsResqml2_0_1Attribute.Name + " attribute of the fesapi/Class Model/" + packageName + "/" + fesapiResqml2Class.Name + "!");
                        return;
                    }
                    if (addResqml2Getter(fesapiResqml2Class, getterName + "AsString", "std::string", 
                        resqml2_0_1AttributeAccessExpression,
                        resqml2_2AttributeAccessExpression,
                        "res = " + Tool.getGsoapEnum2SConverterName(repository, resqml2_0_1AttributeType) + "(" + gsoapResqml2_0_1Proxy + "->soap, " + resqml2_0_1AttributeAccessExpression + ")",
                        "res = " + resqml2_2AttributeAccessExpression) == null)
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
                    resqml2_0_1AttributeAccessExpression.Substring(0, resqml2_0_1AttributeAccessExpression.LastIndexOf("[")),
                    resqml2_2AttributeAccessExpression.Substring(0, resqml2_2AttributeAccessExpression.LastIndexOf("[")),
                    "res = " + resqml2_0_1AttributeAccessExpression.Substring(0, resqml2_0_1AttributeAccessExpression.LastIndexOf("[")) + ".size()",
                    "res = " + resqml2_2AttributeAccessExpression.Substring(0, resqml2_2AttributeAccessExpression.LastIndexOf("[")) + ".size()") == null)
                {
                    Tool.log(repository, "Unable to properly add count getter for the " + energisticsResqml2_0_1Attribute.Name + " attribute of the fesapi/Class Model/" + packageName + "/" + fesapiResqml2Class.Name + "!");
                }
            }
            else if (!(energisticsResqml2_0_1Attribute.UpperBound.Equals("1")))
            {
                if (addResqml2Getter(fesapiResqml2Class, getterName + "Count", "unsigned int",
                    resqml2_0_1AttributeAccessExpression.Substring(0, resqml2_0_1AttributeAccessExpression.LastIndexOf("[")),
                    "",
                    "res = " + resqml2_0_1AttributeAccessExpression.Substring(0, resqml2_0_1AttributeAccessExpression.LastIndexOf("[")) + ".size()",
                    "res = 1") == null)
                {
                    Tool.log(repository, "Unable to properly add count getter for the " + energisticsResqml2_0_1Attribute.Name + " attribute of the fesapi/Class Model/" + packageName + "/" + fesapiResqml2Class.Name + "!");
                }
            }
            else if (!(energisticsResqml2_2Attribute.UpperBound.Equals("1")))
            {
                if (addResqml2Getter(fesapiResqml2Class, getterName + "Count", "unsigned int",
                    "",
                    resqml2_2AttributeAccessExpression.Substring(0, resqml2_2AttributeAccessExpression.LastIndexOf("[")),
                    "res = 1",
                    "res = " + resqml2_2AttributeAccessExpression.Substring(0, resqml2_2AttributeAccessExpression.LastIndexOf("[")) + ".size()") == null)
                {
                    Tool.log(repository, "Unable to properly add count getter for the " + energisticsResqml2_0_1Attribute.Name + " attribute of the fesapi/Class Model/" + packageName + "/" + fesapiResqml2Class.Name + "!");
                }
            }
        }

        EA.Method addResqml2Getter(
            EA.Element fesapiClass, 
            string getterName, 
            string returnType, 
            string resqml2_0_1AttributeAccessExpression, 
            string resqml2_2AttributeAccessExpression, 
            string resqml2_0_1ReturnExpression, 
            string resqml2_2ReturnExpression)
        {
            EA.Method getter = fesapiClass.Methods.AddNew(getterName, returnType);

            getter.Code = returnType + " res;\n";
            getter.Code += "if (gsoapProxy2_0_1 != nullptr)\n";
            getter.Code += "{\n";
            getter.Code += generateAttributeAccessCode(resqml2_0_1AttributeAccessExpression, resqml2_0_1ReturnExpression);
            getter.Code += "}\n";
            getter.Code += "else if (gsoapProxy2_2 != nullptr)\n";
            getter.Code += "{\n";
            getter.Code += generateAttributeAccessCode(resqml2_2AttributeAccessExpression, resqml2_2ReturnExpression);
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

            int resqml2_0_1IndexCount = Tool.countStringOccurrences(resqml2_0_1AttributeAccessExpression, "index");
            int resqml2_2IndexCount = Tool.countStringOccurrences(resqml2_2AttributeAccessExpression, "index");
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

        #region top level relations

        void generateTopLevelRelation(
            EA.Element fesapiSourceClass, 
            EA.Element fesapiDestClass, 
            EA.Element energisticsResqml2_0_1SourceClass, 
            EA.Element energisticsResqml2_2SourceClass,
            string resqml2_0_1AttributeAccessExpression,
            string resqml2_2AttributeAccessExpression,
            string unprefixedMethodName)
        {
            string fesapiSourceClassPackageName = repository.GetPackageByID(fesapiSourceClass.PackageID).Name;
            string fesapiDestClassPackageName = repository.GetPackageByID(fesapiDestClass.PackageID).Name;

            // *******************************************************
            // adding backward relation attribute in destination class       

            if (!backwardRelationSet.ContainsKey(fesapiDestClass.ElementID))
            {
                backwardRelationSet.Add(fesapiDestClass.ElementID, new SortedSet<int>());
            }

            if (!backwardRelationSet[fesapiDestClass.ElementID].Contains(fesapiSourceClass.ElementID))
            {
                if (addBackwardRelationAttribute(fesapiSourceClass, fesapiDestClass) == null)
                {
                    Tool.log(repository, "Unable to properly add backward relation attribute from " + fesapiDestClassPackageName + "/" + fesapiDestClass.Name + " to " + fesapiSourceClassPackageName + "/" + fesapiSourceClass.Name + "!");
                    return;
                }
                backwardRelationSet[fesapiDestClass.ElementID].Add(fesapiSourceClass.ElementID);

                // ***********************************
                // Adding backward relation getter set

                if (addBackwardRelationSetGetter(fesapiSourceClass, fesapiDestClass) == null)
                {
                    Tool.log(repository, "Unable to properly add backward relation set getter fom " + fesapiDestClassPackageName + "/" + fesapiDestClass.Name + " to " + fesapiSourceClassPackageName + "/" + fesapiSourceClass.Name + "!");
                    return;
                }

                // ***********************************
                // Adding backward relation getter set

                if (addBackwardRelationSetCountGetter(fesapiSourceClass, fesapiDestClass) == null)
                {
                    Tool.log(repository, "Unable to properly add backward relation set count getter fom " + fesapiDestClassPackageName + "/" + fesapiDestClass.Name + " to " + fesapiSourceClassPackageName + "/" + fesapiSourceClass.Name + "!");
                    return;
                }

                // *******************************
                // Adding backward relation getter

                if (addBackwardRelationGetter(fesapiSourceClass, fesapiDestClass) == null)
                {
                    Tool.log(repository, "Unable to properly add backward relation getter fom " + fesapiDestClassPackageName + "/" + fesapiDestClass.Name + " to " + fesapiSourceClassPackageName + "/" + fesapiSourceClass.Name + "!");
                    return;
                }
            }

            // ************************************************
            // Adding forward relation setter in source class
            // together with friend setter in destination class

            if (addRelationSetter(fesapiSourceClass, fesapiDestClass, energisticsResqml2_0_1SourceClass, energisticsResqml2_2SourceClass, "set" + Tool.upperCaseFirstLetter(unprefixedMethodName)) == null)
            {
                Tool.log(repository, "Unable to properly add forward relation setter from " + fesapiSourceClassPackageName + "/" + fesapiSourceClass.Name + " to " + fesapiDestClassPackageName + "/" + fesapiDestClass + "!");
                return;
            }

            // **************************************************
            // Adding XML forward relation setter in source class

            if (addXmlRelationSetter(fesapiSourceClass, fesapiDestClass, resqml2_0_1AttributeAccessExpression, resqml2_2AttributeAccessExpression, "set" + Tool.upperCaseFirstLetter(unprefixedMethodName) + "InXml") == null)
            {
                Tool.log(repository, "Unable to properly add XML forward relation setter from " + fesapiSourceClassPackageName + "/" + fesapiSourceClass.Name + " to " + fesapiDestClassPackageName + "/" + fesapiDestClass + "!");
                return;
            }

            // ***************************************************
            // Adding destination class DOR getter in source class
            if (addDestClassDORGetter(fesapiSourceClass, resqml2_0_1AttributeAccessExpression, resqml2_2AttributeAccessExpression, "get" + Tool.upperCaseFirstLetter(unprefixedMethodName) + "Dor") == null)
            {
                Tool.log(repository, "Unable to properly add forward relation DOR getter from " + fesapiSourceClassPackageName + "/" + fesapiSourceClass.Name + " to " + fesapiDestClassPackageName + "/" + fesapiDestClass + "!");
                return;
            }
            
            // ****************************************************
            // Adding destination class UUID getter in source class
            if (addDestClassUuidGetter(fesapiSourceClass, "get" + Tool.upperCaseFirstLetter(unprefixedMethodName) + "Uuid") == null)
            {
                Tool.log(repository, "Unable to properly add forward relation UUID getter from " + fesapiSourceClassPackageName + "/" + fesapiSourceClass.Name + " to " + fesapiDestClassPackageName + "/" + fesapiDestClass + "!");
                return;
            }

            // ***********************************************
            // Adding destination class getter in source class

            if (addDestClassGetter(fesapiSourceClass, fesapiDestClass, "get" + Tool.upperCaseFirstLetter(unprefixedMethodName)) == null)
            {
                Tool.log(repository, "Unable to properly add forward relation getter from " + fesapiSourceClassPackageName + "/" + fesapiSourceClass.Name + " to " + fesapiDestClassPackageName + "/" + fesapiDestClass + "!");
                return;
            }

            // ******************************************************************************
            // Adding required include directive to handle friend setter in destination class

            addOrUpdateFesapiIncludeTag(fesapiDestClass, "#include \"" + repository.GetPackageByID(fesapiSourceClass.PackageID).Name + "/" + fesapiSourceClass.Name + ".h\";");

            //EA.TaggedValue fesapiBackwardIncludeTag = fesapiDestClass.TaggedValues.GetByName(Constants.fesapiIncludeTag);

            //if (fesapiBackwardIncludeTag == null)
            //{
            //    fesapiBackwardIncludeTag = fesapiDestClass.TaggedValues.AddNew(Constants.fesapiIncludeTag, "");
            //}

            //fesapiBackwardIncludeTag.Value += "#include \"" + fesapiSourceClassNamespace + "/" + fesapiSourceClass.Name + ".h\";";
            //if (!fesapiBackwardIncludeTag.Update())
            //{
            //    Tool.showMessageBox(repository, fesapiBackwardIncludeTag.GetLastError());
            //}

            // *************************************************
            // Adding required include directive in source class

            addOrUpdateFesapiIncludeTag(fesapiSourceClass, "#include \"" + repository.GetPackageByID(fesapiDestClass.PackageID).Name + "/" + fesapiDestClass.Name + ".h\";");

            //EA.TaggedValue fesapiForwardIncludeTag = fesapiSourceClass.TaggedValues.GetByName(Constants.fesapiIncludeTag);

            //if (fesapiForwardIncludeTag == null)
            //{
            //    fesapiForwardIncludeTag = fesapiSourceClass.TaggedValues.AddNew(Constants.fesapiIncludeTag, "");
            //}

            //fesapiForwardIncludeTag.Value += "#include \"" + fesapiDestClassNamespace + "/" + fesapiDestClass.Name + ".h\";";
            //if (!fesapiForwardIncludeTag.Update())
            //{
            //    Tool.showMessageBox(repository, fesapiForwardIncludeTag.GetLastError());
            //}
        }

        EA.Attribute addBackwardRelationAttribute(EA.Element fesapiSourceClass, EA.Element fesapiDestClass)
        {
            string backRelAttName = Tool.lowerCaseFirstLetter(fesapiSourceClass.Name) + "Set";

            string fesapiNameSsace = "";
            string fesapiSourceClassNamespace = repository.GetPackageByID(fesapiSourceClass.PackageID).Name;
            string fesapiDestClassNamespace = repository.GetPackageByID(fesapiDestClass.PackageID).Name;
            if (fesapiSourceClassNamespace != fesapiDestClassNamespace)
            {
                fesapiNameSsace = fesapiSourceClassNamespace + "::";
            }

            string backRelAttType = "std::vector<" + fesapiNameSsace + fesapiSourceClass.Name + "*>";
            EA.Attribute backRelAtt = fesapiDestClass.Attributes.AddNew(backRelAttName, backRelAttType);
            backRelAtt.Visibility = "Protected";
            if (!(backRelAtt.Update()))
            {
                Tool.showMessageBox(repository, backRelAtt.GetLastError());
                return null;
            }
            fesapiDestClass.Attributes.Refresh();

            Tool.log(repository, backRelAttName + ":" + backRelAttType + " added to " + fesapiDestClass.Name);

            return backRelAtt;
        }

        EA.Method addBackwardRelationSetGetter(EA.Element fesapiSourceClass, EA.Element fesapiDestClass)
        {
            string fesapiNameSsace = "";
            string fesapiSourceClassNamespace = repository.GetPackageByID(fesapiSourceClass.PackageID).Name;
            string fesapiDestClassNamespace = repository.GetPackageByID(fesapiDestClass.PackageID).Name;
            if (fesapiSourceClassNamespace != fesapiDestClassNamespace)
            {
                fesapiNameSsace = fesapiSourceClassNamespace + "::";
            }

            EA.Method getter = fesapiDestClass.Methods.AddNew("get" + fesapiSourceClass.Name + "Set", "std::vector<" + fesapiNameSsace + fesapiSourceClass.Name + "*>");

            getter.Visibility = "public";
            getter.Stereotype = "const";

            getter.Code = "return " + Tool.lowerCaseFirstLetter(fesapiSourceClass.Name) + "Set;";

            if (!(getter.Update()))
            {
                Tool.showMessageBox(repository, getter.GetLastError());
                return null;
            }
            fesapiSourceClass.Methods.Refresh();

            EA.MethodTag bodyLocationTag = getter.TaggedValues.AddNew("bodyLocation", "classBody");
            if (!(bodyLocationTag.Update()))
            {
                Tool.showMessageBox(repository, bodyLocationTag.GetLastError());
                return null;
            }
            getter.TaggedValues.Refresh();

            return getter;
        }

        EA.Method addBackwardRelationSetCountGetter(EA.Element fesapiSourceClass, EA.Element fesapiDestClass)
        {
            string fesapiNameSpace = "";
            string fesapiSourceClassNamespace = repository.GetPackageByID(fesapiSourceClass.PackageID).Name;
            string fesapiDestClassNamespace = repository.GetPackageByID(fesapiDestClass.PackageID).Name;
            if (fesapiSourceClassNamespace != fesapiDestClassNamespace)
            {
                fesapiNameSpace = fesapiSourceClassNamespace + "::";
            }

            EA.Method getter = fesapiDestClass.Methods.AddNew("get" + fesapiSourceClass.Name + "Count", "unsigned int");

            getter.Visibility = "public";
            getter.Stereotype = "const";

            getter.Code = "return " + Tool.lowerCaseFirstLetter(fesapiSourceClass.Name) + "Set.size();";

            if (!(getter.Update()))
            {
                Tool.showMessageBox(repository, getter.GetLastError());
                return null;
            }
            fesapiSourceClass.Methods.Refresh();

            EA.MethodTag bodyLocationTag = getter.TaggedValues.AddNew("bodyLocation", "classBody");
            if (!(bodyLocationTag.Update()))
            {
                Tool.showMessageBox(repository, bodyLocationTag.GetLastError());
                return null;
            }
            getter.TaggedValues.Refresh();

            return getter;
        }

        EA.Method addBackwardRelationGetter(EA.Element fesapiSourceClass, EA.Element fesapiDestClass)
        {
            string fesapiNameSpace = "";
            string fesapiSourceClassNamespace = repository.GetPackageByID(fesapiSourceClass.PackageID).Name;
            string fesapiDestClassNamespace = repository.GetPackageByID(fesapiDestClass.PackageID).Name;
            if (fesapiSourceClassNamespace != fesapiDestClassNamespace)
            {
                fesapiNameSpace = fesapiSourceClassNamespace + "::";
            }

            EA.Method getter = fesapiDestClass.Methods.AddNew("get" + fesapiSourceClass.Name + "", fesapiNameSpace + fesapiSourceClass.Name + "*");

            getter.Visibility = "public";
            getter.Stereotype = "const";

            getter.Code = "if (" + Tool.lowerCaseFirstLetter(fesapiSourceClass.Name) + "Set.size() > index) {\n";
            getter.Code += "\treturn " + Tool.lowerCaseFirstLetter(fesapiSourceClass.Name) + "Set[index];\n";
            getter.Code += "}\n";
            getter.Code += "else\n";
            getter.Code += "{\n";
            getter.Code += "\tthrow range_error(\"The index is out of the range of the set of " + fesapiSourceClass.Name +".\");\n";
            getter.Code += "}\n";

            if (!(getter.Update()))
            {
                Tool.showMessageBox(repository, getter.GetLastError());
                return null;
            }
            fesapiSourceClass.Methods.Refresh();

            EA.Parameter parameter = getter.Parameters.AddNew("index", "const unsigned int &");
            if (!(parameter.Update()))
            {
                Tool.showMessageBox(repository, parameter.GetLastError());
                return null;
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

        // TODO: commenter le fait que la friend est également générée
        EA.Method addRelationSetter(
            EA.Element fesapiSourceClass, 
            EA.Element fesapiDestClass, 
            EA.Element energisticsResqml2_0_1SourceClass, 
            EA.Element energisticsResqml2_2SourceClass, 
            string setterName)
        {
            // *********************************
            // Adding the setter in source class

            string setterParameterName = Tool.lowerCaseFirstLetter(fesapiDestClass.Name);

            EA.Method setter = fesapiSourceClass.Methods.AddNew(setterName, "void");

            setter.Visibility = "public";

            setter.Code = "";
            if (energisticsResqml2_0_1SourceClass == null)
            {
                setter.Code += "if (" + Constants.gsoapProxy2_2 + " == nullptr) {\n";
                setter.Code += "\tthrow logic_error(\"" + setterName + " is only implemented in Resqml 2.2.\");\n";
                setter.Code += "}\n";
            }
            else if (energisticsResqml2_2SourceClass == null)
            {
                setter.Code += "if (" + Constants.gsoapProxy2_0_1 + " == nullptr) {\n";
                setter.Code += "\tthrow logic_error(\"" + setterName + " is only implemented in Resqml 2.0.1.\");\n";
                setter.Code += "}\n";
            }
            setter.Code += "if (" + setterParameterName + " == nullptr)\n";
            setter.Code += "\tthrow invalid_argument(\"" + setterParameterName + " cannot be null.\");\n";
            setter.Code += "\n";
            setter.Code += "// EPC\n";
            setter.Code += setterParameterName + "->" + Tool.lowerCaseFirstLetter(fesapiSourceClass.Name) + "Set.push_back(this);\n";
            setter.Code += "\n";
            setter.Code += "// XMl\n";
            setter.Code += "if (updateXml) {\n";
            setter.Code += "\t" + setterName + "InXml(" + setterParameterName + ");\n";
            setter.Code += "}";

            if (!(setter.Update()))
            {
                Tool.showMessageBox(repository, setter.GetLastError());
                return null;
            }
            fesapiSourceClass.Methods.Refresh();

            string fesapiNamespace = "";
            string fesapiSourceClassNamespace = repository.GetPackageByID(fesapiSourceClass.PackageID).Name;
            string fesapiDestClassNamespace = repository.GetPackageByID(fesapiDestClass.PackageID).Name;
            if (fesapiSourceClassNamespace != fesapiDestClassNamespace)
            {
                fesapiNamespace = fesapiDestClassNamespace + "::";
            }

            EA.Parameter setterParameter = setter.Parameters.AddNew(setterParameterName, fesapiNamespace + fesapiDestClass.Name + " *");
            if (!(setterParameter.Update()))
            {
                Tool.showMessageBox(repository, setterParameter.GetLastError());
                return null;
            }
            setter.Parameters.Refresh();

            EA.MethodTag setterBodyLocationTag = setter.TaggedValues.AddNew("bodyLocation", "classBody");
            if (!(setterBodyLocationTag.Update()))
            {
                Tool.showMessageBox(repository, setterBodyLocationTag.GetLastError());
                return null;
            }
            setter.TaggedValues.Refresh();

            // *********************************************
            // Adding the friend setter in destination class

            EA.Method friendSetter = fesapiDestClass.Methods.AddNew(fesapiSourceClass.Name + "::" + setterName, "void");

            friendSetter.Visibility = "protected";
            friendSetter.Stereotype = "friend";

            if (!(friendSetter.Update()))
            {
                Tool.showMessageBox(repository, friendSetter.GetLastError());
                return null;
            }
            fesapiDestClass.Methods.Refresh();

            EA.Parameter friendSetterParameter = friendSetter.Parameters.AddNew(Tool.lowerCaseFirstLetter(fesapiDestClass.Name), fesapiNamespace + fesapiDestClass.Name + " *");
            if (!(friendSetterParameter.Update()))
            {
                Tool.showMessageBox(repository, friendSetterParameter.GetLastError());
                return null;
            }
            friendSetter.Parameters.Refresh();

            // adding a tag sepcifying that the body will be located into the class declaration
            EA.MethodTag friendSetterBodyLocationTag = friendSetter.TaggedValues.AddNew("bodyLocation", "noBody");
            if (!(friendSetterBodyLocationTag.Update()))
            {
                Tool.showMessageBox(repository, friendSetterBodyLocationTag.GetLastError());
                return null;
            }
            friendSetter.TaggedValues.Refresh();

            Tool.log(repository, setter.Name + " added to " + fesapiSourceClass.Name + " and (friend method) " + fesapiDestClass.Name);

            return setter;
        }

        EA.Method addXmlRelationSetter(
            EA.Element fesapiSourceClass,
            EA.Element fesapiDestClass, 
            string resqml2_0_1AttributeAccessExpression,
            string resqml2_2AttributeAccessExpression,
            string setterName)
        {
            // *********************************
            // Adding the setter in source class

            string setterParameterName = Tool.lowerCaseFirstLetter(fesapiDestClass.Name);

            EA.Method setter = fesapiSourceClass.Methods.AddNew(setterName, "void");

            setter.Visibility = "protected";

            setter.Code = "";
            if (resqml2_0_1AttributeAccessExpression == "")
            {
                setter.Code += "if (" + Constants.gsoapProxy2_2 + " != nullptr) {\n";
                setter.Code += generateAttributeAccessCode(resqml2_2AttributeAccessExpression, resqml2_2AttributeAccessExpression + " = " + setterParameterName + "->newEml22Reference()");
                setter.Code += "}\n";
                setter.Code += "else {\n";
                setter.Code += "\tthrow logic_error(\"" + setterName + "is only implemented in Resqml 2.2.\");\n";
                setter.Code += "}\n";
            }
            else if (resqml2_2AttributeAccessExpression == "")
            {
                setter.Code += "if (" + Constants.gsoapProxy2_0_1 + " != nullptr) {\n";
                setter.Code += generateAttributeAccessCode(resqml2_0_1AttributeAccessExpression, resqml2_0_1AttributeAccessExpression + " = " + setterParameterName + "->newEml20Reference()");
                setter.Code += "}\n";
                setter.Code += "else {\n";
                setter.Code += "\tthrow logic_error(\"" + setterName + "is only implemented in Resqml 2.0.1.\");\n";
                setter.Code += "}\n";
            }
            else
            {
                setter.Code += "if (" + Constants.gsoapProxy2_0_1 + " != nullptr) {\n";
                setter.Code += generateAttributeAccessCode(resqml2_0_1AttributeAccessExpression, resqml2_0_1AttributeAccessExpression + " = " + setterParameterName + "->newEml20Reference()");
                setter.Code += "}\n";
                setter.Code += "else if (" + Constants.gsoapProxy2_2 + " != nullptr) {\n";
                setter.Code += generateAttributeAccessCode(resqml2_2AttributeAccessExpression, resqml2_2AttributeAccessExpression + " = " + setterParameterName + "->newEml22Reference()");
                setter.Code += "}\n";
                setter.Code += "else\n";
                setter.Code += "{\n";
                setter.Code += "\tthrow logic_error(\"Not implemented yet\");\n";
                setter.Code += "}";
            }

            if (!(setter.Update()))
            {
                Tool.showMessageBox(repository, setter.GetLastError());
                return null;
            }
            fesapiSourceClass.Methods.Refresh();

            string fesapiNamespace = "";
            string fesapiSourceClassNamespace = repository.GetPackageByID(fesapiSourceClass.PackageID).Name;
            string fesapiDestClassNamespace = repository.GetPackageByID(fesapiDestClass.PackageID).Name;
            if (fesapiSourceClassNamespace != fesapiDestClassNamespace)
            {
                fesapiNamespace = fesapiDestClassNamespace + "::";
            }

            EA.Parameter setterParameter = setter.Parameters.AddNew(setterParameterName, fesapiNamespace + fesapiDestClass.Name + " *");
            if (!(setterParameter.Update()))
            {
                Tool.showMessageBox(repository, setterParameter.GetLastError());
                return null;
            }
            setter.Parameters.Refresh();

            EA.MethodTag setterBodyLocationTag = setter.TaggedValues.AddNew("bodyLocation", "classBody");
            if (!(setterBodyLocationTag.Update()))
            {
                Tool.showMessageBox(repository, setterBodyLocationTag.GetLastError());
                return null;
            }
            setter.TaggedValues.Refresh();

            return setter;
        }

        EA.Method addDestClassDORGetter(
            EA.Element fesapiSourceClass,
            string resqml2_0_1AttributeAccessExpression,
            string resqml2_2AttributeAccessExpression,
            string getterName)
        {
            string type;
            if (resqml2_0_1AttributeAccessExpression != "" && resqml2_2AttributeAccessExpression == "")
            {
                type = "gsoap_resqml2_0_1::eml20__DataObjectReference*";
            }
            else
            {
                type = "gsoap_eml2_2::eml22__DataObjectReference *";
            }

            EA.Method getter = fesapiSourceClass.Methods.AddNew(getterName, type);

            getter.Visibility = "public";
            getter.Stereotype = "const";

            getter.Code = "";
            if (resqml2_0_1AttributeAccessExpression == "")
            {
                getter.Code += "if (" + Constants.gsoapProxy2_2 + " != nullptr) {\n";
                getter.Code += generateAttributeAccessCode(resqml2_2AttributeAccessExpression, "return " + resqml2_2AttributeAccessExpression);
                getter.Code += "}\n";
                getter.Code += "else {\n";
                getter.Code += "\tthrow logic_error(\"" + getterName + "is only implemented in Resqml 2.2.\");\n";
                getter.Code += "}\n";
            }
            else if (resqml2_2AttributeAccessExpression == "")
            {
                getter.Code += "if (" + Constants.gsoapProxy2_0_1 + " != nullptr) {\n";
                getter.Code += generateAttributeAccessCode(resqml2_0_1AttributeAccessExpression, "return " + resqml2_0_1AttributeAccessExpression);
                getter.Code += "}\n";
                getter.Code += "else {\n";
                getter.Code += "\tthrow logic_error(\"" + getterName + "is only implemented in Resqml 2.0.1.\");\n";
                getter.Code += "}\n";
            }
            else
            {
                getter.Code += "if (" + Constants.gsoapProxy2_0_1 + " != nullptr) {\n";
                getter.Code += generateAttributeAccessCode(resqml2_0_1AttributeAccessExpression, "gsoap_resqml2_0_1::eml20__DataObjectReference* eml20Dor = " + resqml2_0_1AttributeAccessExpression);
                getter.Code += "\tgsoap_eml2_2::eml22__DataObjectReference* result = gsoap_eml2_2::soap_new_eml22__DataObjectReference(getGsoapContext(), 1);\n";
                getter.Code += "\tresult->Uuid = eml20Dor->UUID;\n";
                getter.Code += "\tresult->Title = eml20Dor->Title;\n";
                getter.Code += "\tresult->ContentType = eml20Dor->ContentType;\n";
                getter.Code += "\tif (eml20Dor->VersionString != nullptr)\n";
                getter.Code += "\t{\n";
                getter.Code += "\t\tresult->VersionString = gsoap_eml2_2::soap_new_std__string(gsoapProxy2_0_1->soap, 1);\n";
                getter.Code += "\t\tresult->VersionString->assign(*eml20Dor->VersionString);\n";
                getter.Code += "\t}\n";
                getter.Code += "\treturn result;\n";
                getter.Code += "}\n";
                getter.Code += "else if (" + Constants.gsoapProxy2_2 + " != nullptr) {\n";
                getter.Code += generateAttributeAccessCode(resqml2_2AttributeAccessExpression, "return " + resqml2_2AttributeAccessExpression);
                getter.Code += "}\n";
                getter.Code += "else\n";
                getter.Code += "{\n";
                getter.Code += "\tthrow logic_error(\"Not implemented yet\");\n";
                getter.Code += "}";
            }

            if (!(getter.Update()))
            {
                Tool.showMessageBox(repository, getter.GetLastError());
                return null;
            }
            fesapiSourceClass.Methods.Refresh();

            EA.MethodTag bodyLocationTag = getter.TaggedValues.AddNew("bodyLocation", "classBody");
            if (!(bodyLocationTag.Update()))
            {
                Tool.showMessageBox(repository, bodyLocationTag.GetLastError());
                return null;
            }
            getter.TaggedValues.Refresh();

            return getter;
        }

        EA.Method addDestClassUuidGetter(EA.Element fesapiSourceClass, string getterName)
        {
            EA.Method getter = fesapiSourceClass.Methods.AddNew(getterName, "std::string");

            getter.Visibility = "public";
            getter.Stereotype = "const";

            getter.Code = "return " + getterName.Replace("Uuid", "Dor") + "()->Uuid;";

            if (!(getter.Update()))
            {
                Tool.showMessageBox(repository, getter.GetLastError());
                return null;
            }
            fesapiSourceClass.Methods.Refresh();

            EA.MethodTag bodyLocationTag = getter.TaggedValues.AddNew("bodyLocation", "classBody");
            if (!(bodyLocationTag.Update()))
            {
                Tool.showMessageBox(repository, bodyLocationTag.GetLastError());
                return null;
            }
            getter.TaggedValues.Refresh();

            return getter;
        }

        EA.Method addDestClassGetter(
            EA.Element fesapiSourceClass,
            EA.Element fesapiDestClass,
            string getterName)
        {
            string fesapiNamespace = "";
            string fesapiSourceClassNamespace = repository.GetPackageByID(fesapiSourceClass.PackageID).Name;
            string fesapiDestClassNamespace = repository.GetPackageByID(fesapiDestClass.PackageID).Name;
            if (fesapiSourceClassNamespace != fesapiDestClassNamespace)
            {
                fesapiNamespace = fesapiDestClassNamespace + "::";
            }

            EA.Method getter = fesapiSourceClass.Methods.AddNew(getterName, fesapiNamespace + fesapiDestClass.Name + " *");

            getter.Visibility = "public";
            getter.Stereotype = "const";

            getter.Code = "return static_cast<" + fesapiNamespace + fesapiDestClass.Name + "*>(epcDocument->getResqmlAbstractObjectByUuid(" + getterName + "Uuid()));";

            if (!(getter.Update()))
            {
                Tool.showMessageBox(repository, getter.GetLastError());
                return null;
            }
            fesapiSourceClass.Methods.Refresh();

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
