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

        private void updatePackage(EA.Package sourcePackage, EA.Package targetPackage)
        {
            foreach (EA.Element sourceClass in sourcePackage.Elements)
            {
                Tool.log(repository, sourceClass.Name);
                Tool.log(repository, "" + targetPackage.Elements.Count);

                EA.Element targetClass = targetPackage.Elements.GetByName(sourceClass.Name);
                if (targetClass != null)
                {
                    if (sourceClass.Notes != "")
                    {
                        targetClass.Notes = sourceClass.Notes;
                        if (!(targetClass.Update()))
                        {
                            Tool.showMessageBox(repository, targetClass.GetLastError());
                            continue;
                        }
                    }
                }
            }
        }

        #endregion
    }
}
