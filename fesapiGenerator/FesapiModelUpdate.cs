using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace fesapiGenerator
{
    class FesapiModelUpdate
    {
        #region members

        /// <summary>
        /// EA repository.
        /// </summary>
        private EA.Repository repository;

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

        /// <summary>
        /// Custom fesapi model
        /// </summary>
        private EA.Package customFesapiModel;

        /// <summary>
        /// Custom fesapi common package
        /// </summary>
        private EA.Package customFesapiCommonPackage;

        /// <summary>
        /// Custom fesapi resqml2 package
        /// </summary>
        private EA.Package customFesapiResqml2Package;

        /// <summary>
        /// Custom fesapi resqml2_0_1 package
        /// </summary>
        private EA.Package customFesapiResqml2_0_1Package;

        /// <summary>
        /// Custom fesapi resqml2_2 package
        /// </summary>
        private EA.Package customFesapiResqml2_2Package;

        #endregion

        #region constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public FesapiModelUpdate(
            EA.Repository repository,
            EA.Package fesapiModel, EA.Package fesapiCommonPackage, EA.Package fesapiResqml2Package, EA.Package fesapiResqml2_0_1Package, EA.Package fesapiResqml2_2Package,
            EA.Package customFesapiModel, EA.Package customFesapiCommonPackage, EA.Package customFesapiResqml2Package, EA.Package customFesapiResqml2_0_1Package, EA.Package customFesapiResqml2_2Package)
        {
            this.repository = repository;
            this.fesapiModel = fesapiModel;
            this.fesapiCommonPackage = fesapiCommonPackage;
            this.fesapiResqml2Package = fesapiResqml2Package;
            this.fesapiResqml2_0_1Package = fesapiResqml2_0_1Package;
            this.fesapiResqml2_2Package = fesapiResqml2_2Package;
            this.customFesapiModel = customFesapiModel;
            this.customFesapiCommonPackage = customFesapiCommonPackage;
            this.customFesapiResqml2Package = customFesapiResqml2Package;
            this.customFesapiResqml2_0_1Package = customFesapiResqml2_0_1Package;
            this.customFesapiResqml2_2Package = customFesapiResqml2_2Package;
        }

        #endregion

        #region public methods

        /// <summary>
        ///  Generate the fesapi model according to input common and resqml models.
        /// </summary>
        public void updateFesapiModel()
        {
            updatePackage(customFesapiCommonPackage, fesapiCommonPackage);
            updatePackage(customFesapiResqml2Package, fesapiResqml2Package);
            updatePackage(customFesapiResqml2_0_1Package, fesapiResqml2_0_1Package);
            updatePackage(customFesapiResqml2_2Package, fesapiResqml2_2Package);
        }

        #endregion

        #region private methods

        private void updateMethods(EA.Element sourceClass, EA.Element targetClass)
        {
            foreach (EA.Method sourceMethod in sourceClass.Methods)
            {
                EA.Method targetMethod = null;
                foreach (EA.Method currentMethod in targetClass.Methods)
                {
                    if (Tool.areEqualMethods(sourceMethod, currentMethod))
                    {
                        targetMethod = currentMethod;
                        break;
                    }
                }

                // for the time being we only consider methods which not exist in the target class
                if (targetMethod != null)
                    continue;

                // we create a new method based on the source one
                targetMethod = targetClass.Methods.AddNew(sourceMethod.Name, sourceMethod.ReturnType);
                targetMethod.Abstract = sourceMethod.Abstract;
                targetMethod.Behavior = sourceMethod.Behavior;
                targetMethod.ClassifierID = sourceMethod.ClassifierID;
                targetMethod.Code = sourceMethod.Code;
                targetMethod.Concurrency = sourceMethod.Concurrency;
                targetMethod.IsConst = sourceMethod.IsConst;
                targetMethod.IsLeaf = sourceMethod.IsLeaf;
                targetMethod.IsPure = sourceMethod.IsPure;
                targetMethod.IsQuery = sourceMethod.IsQuery;
                targetMethod.IsRoot = sourceMethod.IsRoot;
                targetMethod.IsStatic = sourceMethod.IsStatic;
                targetMethod.IsSynchronized = sourceMethod.IsSynchronized;
                targetMethod.Notes = sourceMethod.Notes;
                targetMethod.ReturnIsArray = sourceMethod.ReturnIsArray;
                targetMethod.StateFlags = sourceMethod.StateFlags;
                targetMethod.Stereotype = sourceMethod.Stereotype;
                targetMethod.StereotypeEx = sourceMethod.StereotypeEx;
                targetMethod.Style = sourceMethod.Style;
                targetMethod.StyleEx = sourceMethod.StyleEx;
                targetMethod.Throws = sourceMethod.Throws;
                targetMethod.Visibility = sourceMethod.Visibility;
                if (!(targetMethod.Update()))
                {
                    Tool.showMessageBox(repository, targetMethod.GetLastError());
                }
                targetClass.Methods.Refresh();

                foreach (EA.MethodTag sourceMethodTag in sourceMethod.TaggedValues)
                {
                    EA.MethodTag targetMethodTag = targetMethod.TaggedValues.AddNew(sourceMethodTag.Name, sourceMethodTag.Value);
                    if (!(targetMethodTag.Update()))
                    {
                        Tool.showMessageBox(repository, targetMethodTag.GetLastError());
                    }
                }
                targetMethod.TaggedValues.Refresh();

                foreach (EA.Parameter sourceMethodParameter in sourceMethod.Parameters)
                {
                    EA.Parameter targetMethodParameter = targetMethod.Parameters.AddNew(sourceMethodParameter.Name, sourceMethodParameter.Type);
                    targetMethodParameter.Alias = sourceMethodParameter.Alias;
                    targetMethodParameter.ClassifierID = sourceMethodParameter.ClassifierID;
                    targetMethodParameter.Default = sourceMethodParameter.Default;
                    targetMethodParameter.IsConst = sourceMethodParameter.IsConst;
                    targetMethodParameter.Kind = sourceMethodParameter.Kind;
                    targetMethodParameter.Notes = sourceMethodParameter.Notes;
                    targetMethodParameter.Position = sourceMethodParameter.Position;
                    targetMethodParameter.Stereotype = sourceMethodParameter.Stereotype;
                    targetMethodParameter.StereotypeEx = sourceMethodParameter.StereotypeEx;
                    targetMethodParameter.Style = sourceMethodParameter.Style;
                    targetMethodParameter.StyleEx = sourceMethodParameter.StyleEx;

                    if (!(targetMethodParameter.Update()))
                    {
                        Tool.showMessageBox(repository, targetMethodParameter.GetLastError());
                    }
                }
                targetMethod.Parameters.Refresh();
            }
        }

        private void updateAttributes(EA.Element sourceClass, EA.Element targetClass)
        {
            foreach (EA.Attribute sourceAttribute in sourceClass.Attributes)
            {
                EA.Attribute targetAttribute = null;
                foreach (EA.Attribute currentAttribute in targetClass.Attributes)
                {
                    if (Tool.areEqualAttributes(sourceAttribute, currentAttribute))
                    {
                        targetAttribute = currentAttribute;
                        break;
                    }
                }

                // for the time being we only consider attributes which do not exist in the target class
                if (targetAttribute != null)
                    continue;

                // we create a new attribute based on the source one
                targetAttribute = targetClass.Attributes.AddNew(sourceAttribute.Name, sourceAttribute.Type);
                if (!(targetAttribute.Update()))
                {
                    Tool.showMessageBox(repository, targetAttribute.GetLastError());
                    continue;
                }
                targetClass.Attributes.Refresh();

                targetAttribute.Alias = sourceAttribute.Alias;
                targetAttribute.AllowDuplicates = sourceAttribute.AllowDuplicates;
                targetAttribute.ClassifierID = sourceAttribute.ClassifierID;
                targetAttribute.Container = sourceAttribute.Container;
                targetAttribute.Containment = sourceAttribute.Containment;
                targetAttribute.Default = sourceAttribute.Default;
                targetAttribute.IsCollection = sourceAttribute.IsCollection;
                targetAttribute.IsConst = sourceAttribute.IsConst;
                targetAttribute.IsDerived = sourceAttribute.IsDerived;
                targetAttribute.IsID = sourceAttribute.IsID;
                targetAttribute.IsOrdered = sourceAttribute.IsOrdered;
                targetAttribute.IsStatic = sourceAttribute.IsStatic;
                targetAttribute.Length = sourceAttribute.Length;
                targetAttribute.LowerBound = sourceAttribute.LowerBound;
                targetAttribute.Notes = sourceAttribute.Notes;
                targetAttribute.Precision = sourceAttribute.Precision;
                targetAttribute.RedefinedProperty = sourceAttribute.RedefinedProperty;
                targetAttribute.Scale = sourceAttribute.Scale;
                targetAttribute.Stereotype = sourceAttribute.Stereotype;
                targetAttribute.StereotypeEx = sourceAttribute.StereotypeEx;
                targetAttribute.Style = sourceAttribute.Style;
                targetAttribute.StyleEx = sourceAttribute.StyleEx;
                targetAttribute.SubsettedProperty = sourceAttribute.SubsettedProperty;
                targetAttribute.Type = sourceAttribute.Type;
                targetAttribute.UpperBound = sourceAttribute.UpperBound;
                targetAttribute.Visibility = sourceAttribute.Visibility;
                if (!(targetAttribute.Update()))
                {
                    Tool.showMessageBox(repository, targetAttribute.GetLastError());
                    continue;
                }

                foreach (EA.AttributeTag sourceAttributeTag in sourceAttribute.TaggedValues)
                {
                    EA.AttributeTag targetAttributeTag = targetAttribute.TaggedValues.AddNew(sourceAttributeTag.Name, sourceAttributeTag.Value);
                    if (!(targetAttributeTag.Update()))
                    {
                        Tool.showMessageBox(repository, targetAttributeTag.GetLastError());
                    }
                }
                targetAttribute.TaggedValues.Refresh();
            }
        }

        private void updatePackage(EA.Package sourcePackage, EA.Package targetPackage)
        {
            foreach (EA.Element sourceClass in sourcePackage.Elements)
            {
                Tool.log(repository, sourceClass.Name);
                Tool.log(repository, "" + targetPackage.Elements.Count);

                EA.Element targetClass = targetPackage.Elements.GetByName(sourceClass.Name);
                if (targetClass != null)
                {
                    // handling notes
                    if (sourceClass.Notes != "")
                    {
                        targetClass.Notes = sourceClass.Notes;
                        if (!(targetClass.Update()))
                        {
                            Tool.showMessageBox(repository, targetClass.GetLastError());
                            continue;
                        }
                    }

                    // handling methods
                    updateMethods(sourceClass, targetClass);

                    // handling attributes
                    updateAttributes(sourceClass, targetClass);
                }
            }
        }

        #endregion
    }
}
