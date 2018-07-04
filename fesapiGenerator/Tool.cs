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
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Diagnostics;

namespace fesapiGenerator
{
    /// <summary>
    /// This class provides usefull methods for the fesapi generator addin
    /// </summary>
    public static class Tool
    {
        /// <summary>
        /// This method writes a message in the fesapi generator output tab
        /// </summary>
        /// <param name="repository">The current repository</param>
        /// <param name="message">A message</param>
        public static void log(EA.Repository repository, string message)
        {
            repository.WriteOutput(Constants.outputTabName, message, 0);
        }

        /// <summary>
        /// This methods writes a message in both the fesapi generator output tab
        /// and in a MessageBox. 
        /// </summary>
        /// <param name="repository">The current repository</param>
        /// <param name="message">A message</param>
        public static void showMessageBox(EA.Repository repository, String message)
        {
            log(repository, message);
            MessageBox.Show(message,
                    "",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation,
                    MessageBoxDefaultButton.Button1);
        }

        /// <summary>
        /// This method looks for a package in a given parent package or creates it if 
        /// it does not exist
        /// </summary>
        /// <param name="repository">The current repository</param>
        /// <param name="parentPackage">A parent package</param>
        /// <param name="name">Name of the package we look for</param>
        /// <returns>Found are newly created package</returns>
        public static EA.Package getOrCreatePackage(EA.Repository repository, EA.Package parentPackage, String name)
        {
            EA.Package newPackage;

            try
            {
                newPackage = parentPackage.Packages.GetByName(name);
            }
            catch (Exception)
            {
                newPackage = parentPackage.Packages.AddNew(name, "");
                if (!(newPackage.Update()))
                {
                    Tool.showMessageBox(repository, newPackage.GetLastError());
                    return null;
                }
                parentPackage.Packages.Refresh();

            }

            return newPackage;
        }

        /// <summary>
        /// This methods looks for the local index of a model in a given repository.
        /// That is to say the index of the model in the models collection of the
        /// repository.
        /// </summary>
        /// <param name="repository">The current repository</param>
        /// <param name="package">The model we look for</param>
        /// <returns>The local index of the model or -1 if the repository does not carry such model</returns>
        public static short getIndex(EA.Repository repository, EA.Package model)
        {
            for (short i = 0; i < repository.Models.Count; ++i)
            {
                if (repository.Models.GetAt(i).PackageGUID == model.PackageGUID)
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// This methods looks for the local index of a package. That is to say the
        /// index of the package in the packages collection of its parent package
        /// </summary>
        /// <param name="parentPackage">A parent package</param>
        /// <param name="childPackage">A child package</param>
        /// <returns>The local index of the child package or -1 if the parent package does not carry such package</returns>
        public static short getIndex(EA.Package parentPackage, EA.Package childPackage)
        {
            for (short i = 0; i < parentPackage.Packages.Count; ++i)
                if (parentPackage.Packages.GetAt(i).PackageGUID == childPackage.PackageGUID)
                    return i;

            return -1;
        }

        /// <summary>
        /// This methods looks for the local index of an element (for instance a class).
        /// That is to say the index of the element in the elements collection of its parent package
        /// </summary>
        /// <param name="parentPackage">A parent package</param>
        /// <param name="childElement">A child package</param>
        /// <returns>The local index the child element or -1 if the parent package does not carry such element</returns>
        public static short getIndex(EA.Package parentPackage, EA.Element childElement)
        {
            for (short i = 0; i < parentPackage.Elements.Count; ++i)
                if (parentPackage.Elements.GetAt(i).ElementGUID == childElement.ElementGUID)
                    return i;

            return -1;
        }

        /// <summary>
        /// This method copies an element (for instance a class) into a target package
        /// note : the EA SDK does not provide a simple way to clone an element. It only
        /// provides a way to clone a package in the same parent package. We use this capabilities
        /// to clone an element from one package to another
        /// </summary>
        /// <param name="repository">The current repository</param>
        /// <param name="sourceElement">An element to copy</param>
        /// <param name="targetPackage">The target package</param>
        /// <returns>The newly-created element or null of the copy fails</returns>
        public static EA.Element copyElement(EA.Repository repository, EA.Element sourceElement, EA.Package targetPackage)
        {
            // we get the parent package of the source element
            EA.Package sourcePackage = repository.GetPackageByID(sourceElement.PackageID);

            // create an temporary package into the parent package
            EA.Package tmpPackage = sourcePackage.Packages.AddNew("tmp", "");
            if (!(tmpPackage.Update()))
                return null;
            sourcePackage.Packages.Refresh();

            // move the source element into the temporary package
            sourceElement.PackageID = tmpPackage.PackageID;
            sourceElement.Update();
            sourcePackage.Elements.Refresh();
            tmpPackage.Elements.Refresh();

            // clone the temporary package
            EA.Package clonePackage = tmpPackage.Clone();
            clonePackage.Update();
            sourcePackage.Packages.Refresh();

            // move back the source element into it original parent package
            sourceElement.PackageID = sourcePackage.PackageID;
            sourceElement.Update();
            tmpPackage.Elements.Refresh();
            sourcePackage.Elements.Refresh();

            // getting the copy of the source element
            EA.Element targetElement = clonePackage.Elements.GetByName(sourceElement.Name);

            // move the copy into the targetPackage
            targetElement.PackageID = targetPackage.PackageID;
            targetElement.Update();
            clonePackage.Elements.Refresh();
            targetPackage.Elements.Refresh();

            // delete the temporary packages
            short tmpPackageIndex = getIndex(sourcePackage, tmpPackage);
            short clonePackageIndex = getIndex(sourcePackage, clonePackage);
            sourcePackage.Packages.Delete(tmpPackageIndex);
            sourcePackage.Packages.Refresh();
            sourcePackage.Packages.Delete(clonePackageIndex);
            sourcePackage.Packages.Refresh();
            repository.RefreshModelView(sourcePackage.PackageID);

            return targetElement;
        }

        /// <summary>
        /// This methods sorts the attributes and methods of each class contained in a
        /// given package. It proceed recursively for each sub-package
        /// The ordering proceedas follow:
        /// - regarding attributes: they are ordered alphabetically
        /// - regarding methods: construtors first, then destructor and then other methods
        ///   sorted alphabetically 
        /// </summary>
        /// <param name="repository">The current repository</param>
        /// <param name="package">A package</param>
        public static void sortAttributesAndMethods(EA.Repository repository, EA.Package package)
        {
            foreach (EA.Element c in package.Elements)
            {
                // sorting the attributes
                int attributePos = 0;
                List<EA.Attribute> attributeList = new List<EA.Attribute>();
                foreach (EA.Attribute attribute in c.Attributes)
                    attributeList.Add(attribute);
                List<EA.Attribute> sortedAttributeList = attributeList.OrderBy(a => a.Name).ToList();
                foreach (EA.Attribute attribute in sortedAttributeList)
                {
                    attribute.Pos = attributePos;
                    if (!(attribute.Update()))
                        Tool.showMessageBox(repository, attribute.GetLastError());
                    attributePos++;
                }

                int methodPos = 0;
                // looking for constructors
                EA.Method des = null;
                foreach (EA.Method method in c.Methods)
                {
                    if (method.Name == c.Name)
                    {
                        method.Pos = methodPos;
                        if (!(method.Update()))
                        {
                            Tool.showMessageBox(repository, method.GetLastError());
                        }
                        methodPos++;
                    }
                }

                // looking for destructor
                foreach (EA.Method method in c.Methods)
                {
                    if (method.Name == "~" + c.Name)
                    {
                        des = method;
                        method.Pos = methodPos;
                        if (!(method.Update()))
                        {
                            Tool.showMessageBox(repository, method.GetLastError());
                        }
                        methodPos++;
                        break;
                    }
                }

                // sorting other methods
                List<EA.Method> methodList = new List<EA.Method>();
                foreach (EA.Method method in c.Methods)
                {
                    if (method.Name != c.Name && method.Name != "~" + c.Name)
                        methodList.Add(method);
                }
                List<EA.Method> sortedMethodList = methodList.OrderBy(m => m.Name).ToList();
                foreach (EA.Method method in sortedMethodList)
                {
                    method.Pos = methodPos;
                    if (!(method.Update()))
                    {
                        Tool.showMessageBox(repository, method.GetLastError());
                    }
                    methodPos++;
                }
            }

            // proceed recursively
            foreach (EA.Package p in package.Packages)
                sortAttributesAndMethods(repository, p);
        }

        /// <summary>
        /// This method fills a pre-instantied list with the elements which are tagged 
        /// fesapiGeneration = true in a given package. It proceed recursively for each sub-package
        /// </summary>
        /// <param name="package">A package</param>
        /// <param name="list">A pre-instantied list</param>
        public static void fillFesapiGenerationTaggedElementList(EA.Package package, List<EA.Element> list)
        {
            foreach (EA.Element c in package.Elements)
            {
                EA.TaggedValue fesapiGenerationTag = c.TaggedValues.GetByName(Constants.fesapiGenerationTagName);
                if (fesapiGenerationTag != null)
                    if (fesapiGenerationTag.Value == "true")
                        list.Add(c);
            }

            // proceed recursively
            foreach (EA.Package p in package.Packages)
                fillFesapiGenerationTaggedElementList(p, list);
        }

        /// <summary>
        /// This method fills a pre-instantied list with the elements carried in a given
        /// package. It proceed recursively for each sub-package. Type of element can be
        /// filtered. When filtering classes, one can choose to filter only top level
        /// classes.
        /// </summary>
        /// <param name="package">A package</param>
        /// <param name="list">A pre-instantied list</param>
        /// <param name="type">A type of element to consider (empty string if none)</param>
        /// <param name="matchTopLevelClassOnly">true for filtering only top level classes, else false. This parameter is read only iff type == "class".</param>
        public static void fillElementList(EA.Package package, List<EA.Element> list, string type = "", bool matchTopLevelClassOnly = false)
        {
            if (type == "")
            {
                foreach (EA.Element c in package.Elements)
                    list.Add(c);
            }
            else
            {
                foreach (EA.Element c in package.Elements)
                {
                    if (c.Type.Equals(type))
                    {
                        if (matchTopLevelClassOnly)
                        {
                            if (Tool.isTopLevelClass(c))
                            {
                                list.Add(c);
                            }
                        }
                        else
                        {
                            list.Add(c);
                        }
                    } 
                }
            }

            // proceed recursively
            foreach (EA.Package p in package.Packages)
                fillElementList(p, list, type, matchTopLevelClassOnly);
        }

        /// <summary>
        /// Test if one given class is a top level one. Rule: a class is top level
        /// iff it contains the stereotype "XSDtopLevelElement" or at least one of his 
        /// ancestors is top level. 
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public static bool isTopLevelClass(EA.Element c)
        {
            if (c.Type != "Class")
                return false;

            if (c.Stereotype.Contains("XSDtopLevelElement") || c.StereotypeEx.Contains("XSDtopLevelElement"))
                return true;

            // if c is not stereotyped with XSDtopLevelElement and it has no parent, it is not a top level class
            if (c.BaseClasses.Count == 0)
                return false;

            // if one of the c ancestor is a top level class, c is a top level class
            foreach (EA.Element parentClass in c.BaseClasses)
            {
                if (isTopLevelClass(parentClass))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// This methods asks the user for a generated sources output path. It first looks for
        /// such a path in the addin .xml configuration file
        /// Note: .xml configuration file (when found) should be validated agaisnt an XML schema
        /// </summary>
        /// <returns>The generated sources output path</returns>
        public static string getOutputPath()
        {
            string outputPath = "";

            FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog();
            folderBrowserDialog.Description = "Please select fesapi sources destination folder:";

            string configFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + Constants.configurationFilePath;
            XmlDocument configXmlDoc = new XmlDocument();
            XmlNode outputPathNode = null;
            try
            {
                configXmlDoc.Load(configFilePath);
                outputPathNode = configXmlDoc.DocumentElement.SelectSingleNode("/configuration/outputPath");
                folderBrowserDialog.SelectedPath = outputPathNode.InnerText;
            }
            catch (Exception)
            {
            }

            DialogResult result = folderBrowserDialog.ShowDialog();
            if (result == DialogResult.OK)
            {
                outputPath = folderBrowserDialog.SelectedPath;

                if (outputPathNode == null)
                {
                    XmlNode rootNode;
                    try
                    {
                        rootNode = configXmlDoc.CreateElement("configuration");
                        configXmlDoc.AppendChild(rootNode);
                    }
                    catch (Exception)
                    {
                        rootNode = configXmlDoc.SelectSingleNode("configuration");
                    }

                    outputPathNode = configXmlDoc.CreateElement("outputPath");
                    rootNode.AppendChild(outputPathNode);
                }

                outputPathNode.InnerText = outputPath;
                configXmlDoc.Save(configFilePath);
            }

            return outputPath;
        }

        /// <summary>
        /// This method gets a handle over the main EA window. It is used in order to
        /// set the EA main window as owner of the progress bar dialog.
        /// </summary>
        /// <returns>A handle over the EA main window</returns>
        static public IntPtr getMainWindowHandle()
        {
            Process proc = System.Diagnostics.Process.GetCurrentProcess();
            if (proc.MainWindowTitle == "")
                return IntPtr.Zero;
            else
                return proc.MainWindowHandle;
        }

        static public string uppercaseFirstLetter(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }

            return char.ToUpper(s[0]) + s.Substring(1);
        }

        //TODO: for the time being we consider that two methods are equal iff they have the same name
        static public bool areEqualMethods(EA.Method m1, EA.Method m2)
        {
            return m1.Name.Equals(m2.Name);
        }

        //TODO: for the time being we consider that to attributes are equal iff they have the same name
        static public bool areEqualAttributes(EA.Attribute a1, EA.Attribute a2)
        {
            return a1.Name.Equals(a2.Name);
        }

        static public bool isAbstract(EA.Element c)
        {
            return (c.Type == "Class" && c.Name.StartsWith("Abstract"));
        }

        static public bool isLeaf(EA.Element c)
        {
            foreach (EA.Connector connector in c.Connectors)
            {
                if (connector.Type.Equals("Generalization") && (connector.SupplierID == c.ElementID))
                {
                    return false;
                }
            }

            return true;
        }


        static public bool isMeasureType(EA.Element type)
        {
            if (type == null || type.BaseClasses.Count != 1)
            {
                return false;
            }

            EA.Element baseType = type.BaseClasses.GetAt(0);

            return (baseType.Name.Equals("Measure") || baseType.Name.Equals("AbstractMeasure"));
        }

        //TODO:use proxy generation mapping configuration
        static public string isBasicType(string type)
        {
            if (type.Equals("int") || type.Equals("double") || type.Equals("long") || type.Equals("string"))
                return type;

            if (type.Equals("integer"))
                return "int";

            
            if (type.Equals("positiveInteger") || type.Equals("PositiveInteger") || type.Equals("nonNegativeInteger") || type.Equals("NonNegativeInteger") || type.Equals("positiveLong") || type.Equals("PositiveLong"))
                return "ULONG64";

            if (type.Equals("boolean"))
                return "bool";

            if (type.Equals("anyURI"))
                return "string";

            return "";
        }

        static public string isBasicTypeRec(EA.Repository repository, EA.Element type)
        {
            int targetId = -1;

            foreach (EA.Connector currentConnector in type.Connectors)
            {
                if (currentConnector.Type.Equals("Generalization") && (currentConnector.ClientID == type.ElementID))
                {
                    targetId = currentConnector.SupplierID;
                    break;
                }
            }

            if (targetId == -1)
            {
                return isBasicType(type.Genlinks.Replace("Parent=","").Replace(";",""));
            }
            else
            {
                return isBasicTypeRec(repository, repository.GetElementByID(targetId));
            }
        }
        
        static public string isBasicType(EA.Repository repository, EA.Element type)
        {
            if (!(type.Stereotype.Equals("XSDsimpleType") || type.StereotypeEx.Contains("XSDsimpleType")))
                return "";

            else return isBasicTypeRec(repository, type);
        }

        static public string getBasicType(EA.Repository repository, EA.Attribute attribute)
        {
            string basicType = isBasicType(attribute.Type);
            if (basicType != "")
            {
                return basicType;
            }

            if (attribute.ClassifierID != 0)
            {
                EA.Element typeClass = repository.GetElementByID(attribute.ClassifierID);

                return isBasicType(repository, typeClass);
            }
            else
            {
                return "";
            }
        }

        static private string getGsoapNameRec(EA.Repository repository, EA.Element element, EA.Package package)
        {
            if (package.Name.Equals("v2.0") && repository.GetPackageByID(package.ParentID).Name.Equals("common"))
            {
                if (element.Type == "Class" && !(element.Name.Contains("Abstract")) && !(isEnum(element)))
                {
                    return "gsoap_resqml2_0_1::_eml20__" + element.Name;
                }
                else
                {
                    return "gsoap_resqml2_0_1::eml20__" + element.Name;
                }
            }

            if (package.Name.Equals("v2.2") && repository.GetPackageByID(package.ParentID).Name.Equals("common"))
            {
                if (element.Type == "Class" && !(element.Name.Contains("Abstract")) && !(isEnum(element)))
                {
                    return "gsoap_eml2_2::_eml22__" + element.Name;
                }
                else
                {
                    return "gsoap_eml2_2::eml22__" + element.Name;
                }
            }

            if (package.Name.Equals("v2.0.1") && repository.GetPackageByID(package.ParentID).Name.Equals("resqml"))
            {
                if (element.Type == "Class" && !(element.Name.Contains("Abstract")) && !(isEnum(element)))
                {
                    return "gsoap_resqml2_0_1::_resqml2__" + element.Name;
                }
                else
                {
                    return "gsoap_resqml2_0_1::resqml2__" + element.Name;
                }
            }

            if (package.Name.Equals("v2.2") && repository.GetPackageByID(package.ParentID).Name.Equals("resqml"))
            {
                if (element.Type == "Class" && !(element.Name.Contains("Abstract")) && !(isEnum(element)))
                {
                    return "gsoap_eml2_2::_resqml2__" + element.Name;
                }
                else
                {
                    return "gsoap_eml2_2::resqml2__" + element.Name;
                }
            }

            //// TODO: handle with exception
            //if (package.ParentID == 0)
            //    ...
           
            return getGsoapNameRec(repository, element, repository.GetPackageByID(package.ParentID));
        }

        static public string getGsoapName(EA.Repository repository, EA.Element element)
        {
            EA.Package package = repository.GetPackageByID(element.PackageID);

            return getGsoapNameRec(repository, element, package);
        }

        static private string getFesapiNamespaceRec(EA.Repository repository, EA.Element element, EA.Package package)
        {
            if (package.Name.Equals(Constants.common2PackageName) && repository.GetPackageByID(package.ParentID).Name.Equals(Constants.commonModelName))
            {
                return Constants.fesapiResqml2_0_1PackageName;
            }

            if (package.Name.Equals(Constants.common2_2PackageName) && repository.GetPackageByID(package.ParentID).Name.Equals(Constants.commonModelName))
            {
                return Constants.fesapiResqml2_2PackageName;
            }

            if (package.Name.Equals(Constants.resqml2_0_1PackageName) && repository.GetPackageByID(package.ParentID).Name.Equals(Constants.resqmlModelName))
            {
                return Constants.fesapiResqml2_0_1PackageName;
            }

            if (package.Name.Equals(Constants.resqml2_2PackageName) && repository.GetPackageByID(package.ParentID).Name.Equals(Constants.resqmlModelName))
            {
                return Constants.fesapiResqml2_2PackageName;
            }

            // TODO: handle with exception
            //if (package.ParentID == 0)
            //    ...

            return getFesapiNamespaceRec(repository, element, repository.GetPackageByID(package.ParentID));
        }

        static public string getFesapiNamespace(EA.Repository repository, EA.Element element)
        {
            EA.Package package = repository.GetPackageByID(element.PackageID);

            return getFesapiNamespaceRec(repository, element, package);
        }

        static private string getGsoapProxyNameRec(EA.Repository repository, EA.Element element, EA.Package package)
        {
            if (package.Name.Equals(Constants.common2PackageName) && repository.GetPackageByID(package.ParentID).Name.Equals(Constants.commonModelName))
            {
                return Constants.gsoapProxy2_0_1;
            }

            if (package.Name.Equals(Constants.common2_2PackageName) && repository.GetPackageByID(package.ParentID).Name.Equals(Constants.commonModelName))
            {
                return Constants.gsoapProxy2_2;
            }

            if (package.Name.Equals(Constants.resqml2_0_1PackageName) && repository.GetPackageByID(package.ParentID).Name.Equals(Constants.resqmlModelName))
            {
                return Constants.gsoapProxy2_0_1;
            }

            if (package.Name.Equals(Constants.resqml2_2PackageName) && repository.GetPackageByID(package.ParentID).Name.Equals(Constants.resqmlModelName))
            {
                return Constants.gsoapProxy2_2;
            }

            // TODO: handle with exception
            //if (package.ParentID == 0)
            //    ...

            return getGsoapProxyNameRec(repository, element, repository.GetPackageByID(package.ParentID));
        }

        static public string getGsoapProxyName(EA.Repository repository, EA.Element element)
        {
            EA.Package package = repository.GetPackageByID(element.PackageID);

            return getGsoapProxyNameRec(repository, element, package);
        }



        static public string getGsoapEnum2SConverterName(EA.Repository repository, EA.Element enumElement)
        {
            return getGsoapName(repository, enumElement).Replace("::", "::soap_") + "2s";
        }

        static public string getGsoapS2EnumConverterName(EA.Repository repository, EA.Element enumElement)
        {
            return getGsoapName(repository, enumElement).Replace("::", "::soap_s2");
        }

        static public EA.Element getEnumTypeFromEnumExtType(EA.Repository repository, EA.Element unionElement)
        {
            foreach (EA.Connector currentConnector in unionElement.Connectors)
            {
                if (currentConnector.Type.Equals("Generalization") && (currentConnector.ClientID == unionElement.ElementID))
                {
                    EA.Element targetElement = repository.GetElementByID(currentConnector.SupplierID);
                    if (targetElement.Type.Equals("enumeration") ||
                        targetElement.Type.Equals("Enumeration") || 
                        targetElement.Stereotype.Equals("enumeration") || 
                        targetElement.Stereotype.Equals("Enumeration"))
                    {
                        return targetElement;
                    }
                }
            }

            return null;
        }

        public static bool isEnum(EA.Element type)
        {
            return (type.Type.Equals("enumeration") || type.Type.Equals("Enumeration") || type.Stereotype.Equals("Enumeration") || type.Stereotype.Equals("enumeration") || type.StereotypeEx.Equals("Enumeration") || type.StereotypeEx.Equals("enumeration"));
        }

        public static bool areSameCardinality(string card1, string card2)
        {
            return ((card1 == card2) ||
                (card1 == "1" || card1 == "1..1" || card1 == "") && (card2 == "1" || card2 == "1..1" || card2 == "") ||
                (card1 == "*" || card1 == "0..*") && (card2 == "*" || card2 == "0..*"));
        }

    }
}
