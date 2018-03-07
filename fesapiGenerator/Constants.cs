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

namespace fesapiGenerator
{
    /// <summary>
    /// This class defines some usefull constant values
    /// </summary>
    public static class Constants
    {
        #region models and packages names

        /// <summary>
        /// Name of the Energistics common model
        /// </summary>
        public const string commonModelName = "common";

        /// <summary>
        /// Name of the Energistics common/V2.0 package
        /// </summary>
        public const string commonV2PackageName = "v2.0";

        /// <summary>
        /// Name of the Energistics common/V2.2 package
        /// </summary>
        public const string commonV2_2PackageName = "v2.2";

        /// <summary>
        /// Name of the updated Energistics common/v2.0 package
        /// </summary>
        public const string updatedCommonV2PackageName = "updated v2.0";

        /// <summary>
        /// Name of the Energistics resqml model
        /// </summary>
        public const string resqmlModelName = "resqml";

        /// <summary>
        /// Name of the Energistics resqml/v2.0.1 package
        /// </summary>
        public const string resqmlV2_0_1PackageName = "v2.0.1";

        /// <summary>
        /// Name of the Energistics resqml/v2.2 package
        /// </summary>
        public const string resqmlV2_2PackageName = "v2.2";

        /// <summary>
        /// Name of the updated Energistics resqml/v2.0.1 package
        /// </summary>
        public const string updatedResqmlV2_0_1PackageName = "updated v2.0.1";

        /// <summary>
        /// Name of the fesapi model
        /// </summary>
        public const string fesapiModelName = "fesapi";

        /// <summary>
        /// Name of the fesapi class model
        /// </summary>
        public const string fesapiClassModelName = "Class Model";

        /// <summary>
        /// Name of the fesapi common package
        /// </summary>
        public const string fesapiCommonPackageName = "common";

        /// <summary>
        /// Name of the fesapi resqml2 package
        /// </summary>
        public const string fesapiResqml2PackageName = "resqml2";

        /// <summary>
        /// Name of the fesapi resqml2_0_1 package
        /// </summary>
        public const string fesapiResqml2_0_1PackageName = "resqml2_0_1";

        /// <summary>
        /// Name of the fesapi resqml2_2 package
        /// </summary>
        public const string fesapiResqml2_2PackageName = "resqml2_2";

        /// <summary>
        /// Name of the custom fesapi model
        /// </summary>
        public const string customFesapiModelName = "custom fesapi";

        //TODO: following packages name are same than fesapi ones... 

        /// <summary>
        /// Name of the custom fesapi class model
        /// </summary>
        public const string customFesapiClassModelName = "Class Model";

        /// <summary>
        /// Name of the custom fesapi common package
        /// </summary>
        public const string customFesapiCommonPackageName = "common";

        /// <summary>
        /// Name of the custom fesapi resqml2 package
        /// </summary>
        public const string customFesapiResqml2PackageName = "resqml2";

        /// <summary>
        /// Name of the custom fesapi resqml2_0_1 package
        /// </summary>
        public const string customFesapiResqml2_0_1PackageName = "resqml2_0_1";

        /// <summary>
        /// Name of the custom fesapi resqml2_2 package
        /// </summary>
        public const string customFesapiResqml2_2PackageName = "resqml2_2";

        /// <summary>
        /// Name of the transformed C++ Energistics model
        /// </summary>
        public const string cppEnergisticsModelName = "c++ energistics";

        #endregion

        #region fesapi tags

        /// <summary>
        /// Name of the class or attribute tag telling that a given class or attribute will be generated
        /// </summary>
        public const string fesapiGenerationTagName = "fesapiGeneration";

        /// <summary>
        /// Name of the class tag providing the fesapi namespace
        /// </summary>
        public const string fesapiNamespaceTagName = "fesapiNamespace";

        /// <summary>
        /// Name of the class tag providing the fesapi namespace of the base class
        /// </summary>
        public const string fesapiBaseClassNamespaceTagName = "fesapiBaseClassNamespace";

        /// <summary>
        /// Name of the class tag providing the #include directive pointing on the base class header
        /// </summary>
        public const string fesapiBaseClassIncludeTagName = "fesapiBaseClassIncludeTag";

        /// <summary>
        /// Name of the class tag telling that we should or should not generate an XML_TAG attribute with
        /// its corresponding getter
        /// </summary>
        public const string fesapiGenerateXmlTagTagName = "fesapiGenerateXmlTag";

        /// <summary>
        /// Name of an attribute tag telling that no getter should be automatically generated during
        /// the transformation of the Energistics model into a C++ model
        /// </summary>
        public const string fesapiGetterGenerationTagName = "fesapiGetterGeneration";

        /// <summary>
        /// Name of the EpcDocument class tag used to copy required easy access vector forward
        /// declarations into EpcDocument.h
        /// </summary>
        public const string fesapiImportNamespaceTag = "fesapiImportNamespace";

        #endregion

        /// <summary>
        /// Name of the output tab
        /// </summary>
        public const string outputTabName = "Fesapi Generator";

        /// <summary>
        /// Path to the temporary .xmi file used to save the original Energistics model
        /// prior to an Energistics model update process. The path is relative to the 
        /// user AppData\Roaming\ folder.
        /// </summary>
        public const string tempXmiPath = "\\Sparx Systems\\EA\\Temp\\eaclone.xml";

        /// <summary>
        /// Path to the addin configuration file. The path is relative to the
        /// user AppData\Roaming\ folder
        /// </summary>
        public const string configurationFilePath = "\\Sparx Systems\\EA\\fesapiGeneratorConfiguration.xml";

        /// <summary>
        /// gSOAP prefix for generated Eml v2.0 proxies
        /// </summary>
        public const string eml20GsoapPrefix = "gsoap_resqml2_0_1::eml20__";
        
        /// <summary>
        /// gSOAP prefix for generated Eml v2.2 proxies
        /// </summary>
        public const string eml22GsoapPrefix = "gsoap_eml2_2::eml22__";
        
        /// <summary>
        /// gSOAP prefix for generated Resqml v2.0.1 proxies
        /// </summary>
        public const string resqml2_0_1GsoapPrefix = "gsoap_resqml2_0_1::_resqml2__";

        /// <summary>
        /// gSOAP prefix for generated Resqml v2.2 proxies
        /// </summary>
        public const string resqml2_2GsoapPrefix = "gsoap_eml2_2::_resqml2__";

    }
}
