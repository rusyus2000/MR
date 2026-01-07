<template>
    <div class="modal-backdrop">
        <div class="modal-content shadow modal-wide">
            <div class="modal-header custom-header">
                <h5 class="modal-title d-flex align-items-center gap-2">
                    <span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Product Name *'] || ''" aria-label="Field definition" tabindex="0">
                        <i class="bi bi-info-circle info-icon"></i>
                    </span>
                    <span>{{ isLoading ? 'Loading…' : (item ? item.title : '') }}</span>
                    <button v-if="!isLoading && isAdmin && FEATURE_FLAGS.allowManualEdit" class="btn btn-link text-primary p-0 ms-2" title="Edit Item" @click="$emit('edit')">
                        <i class="bi bi-pencil-square fs-4"></i>
                    </button>
                    <button v-if="!isLoading && item" class="btn btn-sm favorite-toggle-btn" :title="item.isFavorite ? 'Remove from Favorites' : 'Add to Favorites'" @click="toggleFavorite(item)">
                        <i :class="item.isFavorite ? 'bi bi-star-fill' : 'bi bi-star'"></i>
                    </button>
                </h5>
                <div class="d-flex align-items-center">
                    <button class="btn-close" @click="$emit('close')"></button>
                </div>
            </div>
            <div class="modal-body" ref="modalBody">
                <div v-if="showAccessForm" class="request-access-overlay">
                    <div class="request-access-card">
                        <div class="d-flex justify-content-between align-items-center mb-2">
                            <h6 class="mb-0">Request access email</h6>
                            <button class="btn-close" @click="closeAccessForm"></button>
                        </div>
                        <div class="access-row">
                            <label class="form-label access-label">To</label>
                            <input class="form-control" type="email" v-model="accessForm.to" readonly required />
                        </div>
                        <div class="access-row">
                            <label class="form-label access-label">CC</label>
                            <input class="form-control" :class="{ 'is-invalid': isAccessFieldInvalid('cc') }" type="text" v-model="accessForm.cc" placeholder="Enter email of Manager of the users needing access" required />
                        </div>
                        <div class="access-row access-row-users">
                            <label class="form-label access-label">
                                <span class="info-icon-wrap me-1" data-tooltip="List all users who needs access in the following format:&#10;LName1, FName1 (NtLogin1); LName2, FName2 (NTlogin2);" aria-label="Users format" tabindex="0">
                                    <i class="bi bi-info-circle info-icon"></i>
                                </span>
                                Users
                            </label>
                            <textarea class="form-control access-users" :class="{ 'is-invalid': isAccessFieldInvalid('users') }" rows="4" v-model="accessForm.users" required></textarea>
                        </div>
                        <div class="access-row access-row-body">
                            <label class="form-label access-label">
                                <span class="info-icon-wrap me-1" data-tooltip="Detailed reason why users need access and what specific information they need from this report" aria-label="Business reason" tabindex="0">
                                    <i class="bi bi-info-circle info-icon"></i>
                                </span>
                                Business Reason
                            </label>
                            <textarea class="form-control access-body" :class="{ 'is-invalid': isAccessFieldInvalid('businessReason') }" rows="4" v-model="accessForm.businessReason" required></textarea>
                        </div>
                        <div class="d-flex justify-content-end">
                            <div v-if="showAccessValidation" class="text-danger me-3 align-self-center access-required-note">All fields are required.</div>
                            <button class="btn btn-sm btn-primary" :disabled="outlookOpening" @click="openInOutlook">
                                <span v-if="outlookOpening">Openning Outlook{{ outlookDots }}</span>
                                <span v-else>Open in Outlook</span>
                            </button>
                        </div>
                    </div>
                </div>
                <div v-if="isLoading" class="d-flex justify-content-center align-items-center" style="min-height:160px;">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                </div>
                <div v-else class="details-grid">
                    <!-- Required fields block (top) -->
                    <!-- Row 1: Product Name removed (title shown in header) -->

                    <!-- Row 2: Description spans across -->
                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Product Description and Purpose *'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Product Description and Purpose <span class="req">*</span>:</div>
                    <div ref="descEl"
                         class="desc desc-cell desc-auto"
                         :class="{ 'desc-fit-one': descFitsOneLine && !descMeasuring, 'desc-measure': descMeasuring }"
                         :title="(item.description && item.description.trim()) ? item.description : 'Missing Data'">
                        {{ (item.description && item.description.trim()) ? item.description : 'Missing Data' }}
                    </div>

                    <!-- Row 3: Location/URL | Division -->
                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Location/URL *'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Location/URL <span class="req">*</span>:</div>
                    <div class="url-row">
                        <template v-if="item.url && item.url.trim()">
                            <a href="#" @click.prevent="openResource(item)" :title="item.url">
                                {{ shorten(item.url, 40) }}
                            </a>
                            <button class="btn btn-link btn-sm p-0 ms-2 copy-btn" :title="'Copy URL'" @click="copyUrl(item.url)">
                                <i class="bi bi-clipboard"></i>
                            </button>
                        </template>
                        <template v-else>
                            <span class="text-muted">Missing Data</span>
                        </template>
                    </div>
                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Division *'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Division <span class="req">*</span>:</div><div>{{ item.division || 'Missing Data' }}</div>

                    <!-- Row 4: Domain | Operating Entity -->
                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Domain *'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Domain <span class="req">*</span>:</div><div>{{ item.domain || 'Missing Data' }}</div>
                    <!-- Row 5: Operating Entity | Executive Sponsor -->
                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Operating Entity *'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Operating Entity <span class="req">*</span>:</div><div>{{ item.operatingEntity || 'Missing Data' }}</div>
                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Executive Sponsor *'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Executive Sponsor <span class="req">*</span>:</div>
                    <div>
                        <template v-if="item.executiveSponsorEmail">
                            <a :href="`mailto:${item.executiveSponsorEmail}`">{{ item.executiveSponsorName || item.executiveSponsorEmail }}</a>
                        </template>
                        <template v-else>
                            {{ item.executiveSponsorName || 'Missing Data' }}
                        </template>
                    </div>

                    <!-- Row 6: D&A Product Owner | Data Consumers -->
                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['D&A Product Owner *'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>D&amp;A Product Owner <span class="req">*</span>:</div>
                    <div>
                        <template v-if="item.ownerEmail">
                            <a :href="`mailto:${item.ownerEmail}`">{{ item.ownerName || item.ownerEmail }}</a>
                        </template>
                        <template v-else>
                            {{ item.ownerName || 'Missing Data' }}
                        </template>
                    </div>
                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Data Consumers *'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Data Consumers <span class="req">*</span>:</div>
                    <div><span v-if="item.dataConsumers && item.dataConsumers.trim()">{{ item.dataConsumers }}</span><span v-else class="text-muted">Missing Data</span></div>

                    <!-- Row 7: BI Platform | Dependencies -->
                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['BI Platform *'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>BI Platform <span class="req">*</span>:</div><div>{{ item.biPlatform || 'Missing Data' }}</div>
                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Dependencies'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Dependencies <span class="req">*</span>:</div>
                    <div><span v-if="item.dependencies && item.dependencies.trim()">{{ item.dependencies }}</span><span v-else class="text-muted">Missing Data</span></div>

                    <!-- Row 8: Default AD Groups | Flags -->
                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Default AD Group Names *'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Default AD Groups <span class="req">*</span>:</div>
                    <div><span v-if="item.defaultAdGroupNames && item.defaultAdGroupNames.trim()">{{ item.defaultAdGroupNames }}</span><span v-else class="text-muted">Missing Data</span></div>
                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Flags'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Flags <span class="req">*</span>:</div>
                    <div class="flags">
                        <span>PHI: {{ item.privacyPhiDisplay || (item.privacyPhi === null || item.privacyPhi === undefined ? 'Missing Data' : (item.privacyPhi ? 'Yes' : 'No')) }}</span>
                        <span>PII: {{ item.privacyPiiDisplay || (item.privacyPii === null || item.privacyPii === undefined ? 'Missing Data' : (item.privacyPii ? 'Yes' : 'No')) }}</span>
                        <span>RLS: {{ item.hasRlsDisplay || (item.hasRls === null || item.hasRls === undefined ? 'Missing Data' : (item.hasRls ? 'Yes' : 'No')) }}</span>
                    </div>

                    <!-- Row 9: Refresh Frequency | Last Date Modified -->
                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Refresh Frequency *'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Refresh Frequency <span class="req">*</span>:</div><div>{{ item.refreshFrequency || 'Missing Data' }}</div>
                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Last Date Modified for this row *'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Last Date Modified <span class="req">*</span>:</div>
                    <div>{{ item.lastModifiedDate ? new Date(item.lastModifiedDate).toLocaleDateString() : 'Missing Data' }}</div>

                    <!-- End required block -->

                    <!-- Remaining fields -->
                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Asset Type *'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Asset Type:</div>
                    <div>{{ item.assetTypeName }}</div>
                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Product Status'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Status:</div>
                    <div>{{ item.status || '-' }}</div>

                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Data Source *'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Data Source:</div>
                    <div>{{ item.dataSource || 'Missing Data' }}</div>
                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Product Status Notes'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Product Status Notes:</div>
                    <div>{{ item.productStatusNotes || '-' }}</div>

                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Potential to Consolidate'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Potential to Consolidate:</div>
                    <div>{{ item.potentialToConsolidate || '-' }}</div>
                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Product Group'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Product Group:</div>
                    <div>{{ item.productGroup || '-' }}</div>

                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Potential to Automate'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Potential to Automate:</div>
                    <div>{{ item.potentialToAutomate || '-' }}</div>
                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Product Impact Category'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Product Impact Category:</div>
                    <div>{{ item.productImpactCategory || '-' }}</div>

                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Estimated 2025 development hours'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Estimated Dev Hours:</div>
                    <div>{{ item.estimatedDevHours || '-' }}</div>
                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Tech Delivery Mgr'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Tech Delivery Mgr:</div>
                    <div>{{ item.techDeliveryManager || '-' }}</div>

                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Business Value rated by the executive sponsor'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Business Value:</div>
                    <div>{{ item.sponsorBusinessValue || '-' }}</div>
                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Regulatory/Compliance/Contractual Flag*'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Regulatory/Compliance/Contractual:</div>
                    <div>{{ item.regulatoryComplianceContractual || '-' }}</div>

                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Development Effort'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Development Effort:</div>
                    <div>{{ item.developmentEffort || '-' }}</div>
                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['DB Server'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>DB Server:</div>
                    <div>{{ item.dbServer || '-' }}</div>

                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Resources - Development'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Resources - Development:</div>
                    <div>{{ item.resourcesDevelopment || '-' }}</div>
                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['DB/Data Mart'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>DB/Data Mart:</div>
                    <div>{{ item.dbDataMart || '-' }}</div>

                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Resources - Analysts'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Resources - Analysts:</div>
                    <div>{{ item.resourcesAnalysts || '-' }}</div>
                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Database Table'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Database Table:</div>
                    <div>{{ item.databaseTable || '-' }}</div>

                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Resources - Platform'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Resources - Platform:</div>
                    <div>{{ item.resourcesPlatform || '-' }}</div>
                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Source Repo'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Source Rep:</div>
                    <div>{{ item.sourceRep || '-' }}</div>

                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Resources - Data Engineering'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Resources - Data Engineering:</div>
                    <div>{{ item.resourcesDataEngineering || '-' }}</div>
                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['dataSecurityClassification'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Data Security Classification:</div>
                    <div>{{ item.dataSecurityClassification || '-' }}</div>

                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['accessGroupName'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Access Group Name:</div>
                    <div>{{ item.accessGroupName || '-' }}</div>
                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['accessGroupDN'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Access Group DN:</div>
                    <div>{{ item.accessGroupDn || '-' }}</div>

                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Automation Classification'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Automation Classification:</div>
                    <div>{{ item.automationClassification || '-' }}</div>
                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['user_visibility_string'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>User Visibility (String):</div>
                    <div>{{ item.userVisibilityString || '-' }}</div>

                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Epic Security Group tag *'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Epic Security Group Tag:</div>
                    <div>{{ item.epicSecurityGroupTag || '-' }}</div>
                    <div class="label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Keep'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Keep Long Term:</div>
                    <div>{{ item.keepLongTerm || '-' }}</div>

                    <div class="label align-top tags-label"><span class="info-icon-wrap" :data-tooltip="FIELD_DEFINITIONS['Tags'] || ''" aria-label="Field definition" tabindex="0"><i class="bi bi-info-circle info-icon"></i></span>Tags:</div>
                    <div class="tags-value align-top">
                        <div class="tag-list">
                            <template v-if="!item || !item.tags || item.tags.length === 0">
                                <span class="text-muted">None</span>
                            </template>
                            <template v-else>
                                <span v-for="(t, idx) in item.tags" :key="idx" class="tag-chip me-1">
                                    <span class="tag-text">{{ t }}</span>
                                </span>
                            </template>
                        </div>
                    </div>

                    
                </div>
            </div>

            <div v-if="showScrollHint" class="scroll-hint" aria-hidden="true">
                <i class="bi bi-arrow-down-circle-fill"></i>
            </div>
            <div v-if="!isLoading && item" class="d-flex justify-content-end mt-3 px-4 pb-3 gap-2">
                <div v-if="lastAccessRequestedAt" class="access-request-note">
                    Last access request made on {{ formatDateTime(lastAccessRequestedAt) }}
                </div>
                <button class="btn btn-sm btn-outline-teal action-btn" @click="openAccessForm">Request access</button>
                <button class="btn btn-sm btn-outline-secondary action-btn" @click="$emit('close')">Close</button>
            </div>
        </div>
    </div>
</template>

<script>
    import { toggleFavoriteApi, fetchLastAccessRequest, recordAccessRequest } from '../services/api';
    import { FEATURE_FLAGS } from '../config';
    import { getCurrentUserCached } from '../services/api';

    const FIELD_DEFINITIONS = Object.freeze({
        "Domain *": "The top/first logical grouping of the 3 level portfolio framework with related data products that share common functions, features, or intended uses, serving to organize inventory for data customers and facilite efficient discovery.",
        "Product Group": "The middle/second logical grouping of the 3 level portfolio framework with related data products that share common functions, features, or intended uses, serving to organize inventory for data customers and facilite efficient discovery.",
        "Product Name *": "The lowest/third logical grouping of the 3 level portfolio framework on the specific data product for data customers and facilite efficient discovery.",
        "Product Description and Purpose *": "A more detailed description with more context about the data product",
        "Product Status": "Product status defines a product's current availability or lifecycle stage for users to consume or not. (e.g. Active, Active-Approved Standard, Active-legal retained, Decommissioned, Pending - Decommission, Unknown) Product status defines a product's current availability or lifecycle stage for users to consume or not. (e.g. Active - Legacy(existing custom work no longer put more dev efforts post 2025), Active-Approved Standard in Prod (continue support post 2025), Active-legal retained (kept for legal purpose), Decommissioned (removed from invenotry), Pending - Decommission (plan to sunset, will be removed from inventory), Unknown)",
        "Product Status Notes": "Additional explanation about the current product status.",
        "Division *": "This is to identify which Sutter's operating areas the data product is created for. Mostly for legacy products but could have a handful of unique products that could be for specific operating areas/division.",
        "Operating Entity *": "This is for legacy purpose as some of our data products were built specifically for an operating entity.",
        "Executive Sponsor *": "Final decision maker for our data consumers from the business; especially on priority of what should D&A build first.",
        "Data Consumers *": "Users or Group of users who depends on the specific data product to make decisions or take actions as part of their normal workflow.",
        "D&A Product Owner *": "A product owner is a role within an agile product team responsible for maximizing the value of a product or group of products. They act as the voice of the customer. They are the liaison between stakeholders, development teams, and users. Product owner defines the product vision, managing the product backlog, and ensuring the team is focused on delivery business value based on the product vision and strategy.",
        "D&A Product Owner Email *": "The Sutter email address of the D&A product owner",
        "Tech Delivery Mgr": "A technical delivery manager oversees the process of delivering products or services, ensuring they are completed on time, within budget, and meet quality standards. They act as the bridge between product owner and the development team and manages resources, risks, and communication.",
        "Regulatory/Compliance/Contractual Flag*": "A flag to identify if the product is for regulatory, compliance or contractual obligation for updates and delivery",
        "Asset Type *": "A classification that defines a category of data or data-related resource, such as a \"Database,\" \"Table,\" \"Report\" or \"Dashboard\"",
        "BI Platform *": "The software system used to collect, analyze and visualize data to gain insights and make decisions.",
        "Location/URL *": "The specific location or URL where users and developer can access the actual data product.",
        "DB Server": "Identify the SQL server name where the data source is from.",
        "DB/Data Mart": "Identify the datamart name where the data source is from.",
        "Database Table": "Identify the data table name where the data source is from.",
        "Source Repo": "Identify the repo location where the code used to extract, load or transform the data is stored.",
        "dataSecurityClassification": "The label of the data classification from a Sutter security standoint.",
        "accessGroupName": "An IAM group for control access",
        "accessGroupDN": "An IAM group for control access",
        "Data Source *": "This is the true data source that was used to power the BI platform",
        "Automation Classification": "Identify if the process to refresh this data product is fully automated, manual or partially automated. (e.g. Auto, Manual, Partial Auto)",
        "user_visibility_string": "A flag for marketplace",
        "Default AD Group Names *": "An IAM group for control access; typically based on the user's default role or organization.",
        "Flags": "PHI: A flag to indicate the data product contains PHI. PII: A flag to indicate the data product contains PII. RLS: A flag to indicate the product has Row Level Security in place.",
        "Epic Security Group tag *": "Internal Epic access permission (not dependent on AD Group)",
        "Refresh Frequency *": "How often is the product refreshed",
        "Keep": "An indicator identify if this product is intended to be kept on-going beyond 2025",
        "Potential to Consolidate": "A flag to indicate if the product has been identified to be consolidated with another product.",
        "Potential to Automate": "An indicator that thie product is a candidate to be automated.",
        "Last Date Modified for this row *": "Last updated date for this row",
        "Business Value rated by the executive sponsor": "An assessment by the sponsor/stakeholder (High, Low, Medium)",
        "Product Impact Category": "Strategic/Enhancement/KTLO",
        "Development Effort": "Quick T-Shirt sizing by the tech delivery manager (XL, XS, L, M, S)",
        "Estimated 2025 development hours": "Refined point estimation by the product team",
        "Resources - Development": "Tech delivery manager identifies who in the development team has the skills to complete the work",
        "Resources - Analysts": "Tech delivery manager identifies who in the BI&A team has the skills to complete the work",
        "Resources - Platform": "Tech delivery manager identifies who in the BI&A team has the skills to complete the work",
        "Resources - Data Engineering": "Tech delivery manager identifies who in the BI&A team has the skills to complete the work",
        "Dependencies": "Describe upstream data lineage or dependencies",
        "Tags": "Custom tags to help the search functionality"
    });

    export default {
        name: 'ModalAssetDetails',
        props: {
            item: Object,
            isAdmin: { type: Boolean, default: false },
            isLoading: { type: Boolean, default: false }
        },
        emits: ['close','edit'],
        data() {
            return {
                FEATURE_FLAGS,
                FIELD_DEFINITIONS,
                showScrollHint: false,
                _scrollHintTimer: null,
                _scrollHintSeq: 0,
                descFitsOneLine: true,
                descMeasuring: false,
                _descMeasureSeq: 0,
                _descResizeTimer: null,
                showAccessForm: false,
                outlookOpening: false,
                outlookDots: '',
                _outlookTimer: null,
                _outlookStopTimer: null,
                lastAccessRequestedAt: null,
                showAccessValidation: false,
                accessForm: {
                    to: '',
                    cc: '',
                    subject: 'Access Request',
                    requesterName: '',
                    requesterUpn: '',
                    users: '',
                    businessReason: ''
                }
            };
        },
        watch: {
            isLoading() {
                this.maybeShowScrollHint();
                this.scheduleUpdateDescriptionFit();
            },
            item() {
                this.maybeShowScrollHint();
                this.scheduleUpdateDescriptionFit();
                this.syncAccessForm();
                this.loadLastAccessRequest();
            }
        },
        mounted() {
            this.maybeShowScrollHint();
            this.scheduleUpdateDescriptionFit();
            window.addEventListener('resize', this.scheduleUpdateDescriptionFit, { passive: true });
            this.initCurrentUser();
            this.syncAccessForm();
            this.loadLastAccessRequest();
        },
        beforeUnmount() {
            if (this._scrollHintTimer) clearTimeout(this._scrollHintTimer);
            if (this._descResizeTimer) clearTimeout(this._descResizeTimer);
            if (this._outlookTimer) clearInterval(this._outlookTimer);
            if (this._outlookStopTimer) clearTimeout(this._outlookStopTimer);
            window.removeEventListener('resize', this.scheduleUpdateDescriptionFit);
        },
        methods: {
            scheduleUpdateDescriptionFit() {
                if (this._descResizeTimer) clearTimeout(this._descResizeTimer);
                this._descResizeTimer = setTimeout(() => {
                    this._descResizeTimer = null;
                    this.updateDescriptionFit();
                }, 120);
            },
            updateDescriptionFit() {
                const seq = ++this._descMeasureSeq;
                if (this.isLoading) {
                    this.descFitsOneLine = true;
                    this.descMeasuring = false;
                    return;
                }
                const text = this.item && typeof this.item.description === 'string' ? this.item.description.trim() : '';
                if (!text) {
                    this.descFitsOneLine = true;
                    this.descMeasuring = false;
                    return;
                }

                // Measure overflow with a temporary nowrap state to decide whether it fits in 1 line.
                this.descMeasuring = true;
                this.$nextTick(() => {
                    if (seq !== this._descMeasureSeq) return;
                    const el = this.$refs.descEl;
                    if (!el) {
                        this.descMeasuring = false;
                        return;
                    }
                    const overflows = el.scrollWidth > (el.clientWidth + 1);
                    this.descFitsOneLine = !overflows;
                    this.descMeasuring = false;
                });
            },
            maybeShowScrollHint() {
                if (this._scrollHintTimer) clearTimeout(this._scrollHintTimer);
                this.showScrollHint = false;
                if (this.isLoading) return;

                const seq = ++this._scrollHintSeq;
                this.$nextTick(() => {
                    if (seq !== this._scrollHintSeq) return;
                    const el = this.$refs.modalBody;
                    if (!el) return;
                    const canScroll = el.scrollHeight > (el.clientHeight + 8);
                    if (!canScroll) return;

                    this.showScrollHint = true;
                    const blinkDurationMs = 1000;
                    const blinkCount = 4;
                    const totalMs = blinkDurationMs * blinkCount;
                    this._scrollHintTimer = setTimeout(() => {
                        if (seq !== this._scrollHintSeq) return;
                        this.showScrollHint = false;
                        this._scrollHintTimer = null;
                    }, totalMs);
                });
            },
            async initCurrentUser() {
                try {
                    const me = await getCurrentUserCached();
                    const name = me?.displayName || me?.userPrincipalName || '';
                    const nt = me?.userPrincipalName || '';
                    this.accessForm.requesterName = name;
                    this.accessForm.requesterUpn = nt;
                    this.syncAccessForm();
                } catch {
                    // Leave fields editable if user lookup fails
                }
            },
            syncAccessForm() {
                const toEmail = this.item?.executiveSponsorEmail || '';
                this.accessForm.to = toEmail;
                this.accessForm.subject = 'Access Request';
                if (!this.accessForm.users) {
                    this.accessForm.users = this.buildRequesterLineWithSuffix();
                }
                this.showAccessValidation = false;
            },
            buildAccessBody() {
                const lines = [];
                const title = this.item?.title || 'this asset';
                const url = this.item?.url || '';
                const sponsorName = this.item?.executiveSponsorName || 'Executive Sponsor';
                const users = this.parseUsersList();
                const reason = (this.accessForm.businessReason || '').trim();

                lines.push(`Hello ${sponsorName},`);
                lines.push('');
                lines.push('I am requesting access to:');
                lines.push(`        Name: ${title}`);
                if (url) {
                    lines.push(`        URL: ${url}`);
                }
                lines.push('Please grant access for the following user(s):');
                if (users.length > 0) {
                    users.forEach(user => lines.push(`        ${user}`));
                } else {
                    lines.push(`        ${this.buildRequesterLine()}`);
                }
                lines.push('');
                lines.push('Reason for access:');
                lines.push(`       ${reason || '[Add business justification here]'}`);
                lines.push('');
                lines.push('Thank you.');
                return lines.join('\n');
            },
            buildRequesterLine() {
                const upn = this.accessForm.requesterUpn || '';
                const name = this.accessForm.requesterName || '';
                if (name && upn) return `${name} (${upn})`;
                return name || upn || '';
            },
            openAccessForm() {
                this.showAccessForm = true;
                this.showAccessValidation = false;
                if (!this.accessForm.users) {
                    this.accessForm.users = this.buildRequesterLineWithSuffix();
                }
            },
            closeAccessForm() {
                this.showAccessForm = false;
            },
            buildRequesterLineWithSuffix() {
                const line = this.buildRequesterLine();
                return line ? `${line};` : '';
            },
            parseUsersList() {
                const raw = this.accessForm.users || '';
                return raw
                    .split(/[\n;]+/)
                    .map(part => part.trim())
                    .filter(Boolean);
            },
            isAccessFormValid() {
                const to = (this.accessForm.to || '').trim();
                const cc = (this.accessForm.cc || '').trim();
                const users = (this.accessForm.users || '').trim();
                const reason = (this.accessForm.businessReason || '').trim();
                return Boolean(to && cc && users && reason);
            },
            isAccessFieldInvalid(field) {
                if (!this.showAccessValidation) return false;
                const value = (this.accessForm[field] || '').trim();
                return !value;
            },
            openInOutlook() {
                if (!this.isAccessFormValid()) {
                    this.showAccessValidation = true;
                    return;
                }
                this.showAccessValidation = false;
                const to = (this.accessForm.to || '').trim();
                const params = [];
                const cc = (this.accessForm.cc || '').trim();
                if (cc) params.push(`cc=${encodeURIComponent(cc)}`);
                const subject = this.accessForm.subject || 'Access Request';
                params.push(`subject=${encodeURIComponent(subject)}`);
                const body = this.buildAccessBody();
                params.push(`body=${encodeURIComponent(body)}`);
                const qs = params.join('&');
                const mailto = `mailto:${to}${qs ? `?${qs}` : ''}`;
                this.startOutlookSpinner();
                this.recordAccessRequest();
                window.location.href = mailto;
            },
            async loadLastAccessRequest() {
                if (!this.item || !this.item.id) {
                    this.lastAccessRequestedAt = null;
                    return;
                }
                try {
                    const res = await fetchLastAccessRequest(this.item.id);
                    this.lastAccessRequestedAt = res?.requestedAt || null;
                } catch {
                    this.lastAccessRequestedAt = null;
                }
            },
            async recordAccessRequest() {
                if (!this.item || !this.item.id) return;
                try {
                    const res = await recordAccessRequest(this.item.id);
                    this.lastAccessRequestedAt = res?.requestedAt || this.lastAccessRequestedAt;
                } catch { }
            },
            formatDateTime(value) {
                if (!value) return '';
                const d = new Date(value);
                const mm = String(d.getMonth() + 1).padStart(2, '0');
                const dd = String(d.getDate()).padStart(2, '0');
                const yy = String(d.getFullYear()).slice(-2);
                const hh = String(d.getHours()).padStart(2, '0');
                const min = String(d.getMinutes()).padStart(2, '0');
                return `${mm}/${dd}/${yy} ${hh}:${min}`;
            },
            startOutlookSpinner() {
                if (this.outlookOpening) return;
                this.outlookOpening = true;
                this.outlookDots = '\u00A0\u00A0\u00A0';
                if (this._outlookTimer) clearInterval(this._outlookTimer);
                if (this._outlookStopTimer) clearTimeout(this._outlookStopTimer);

                const dotStates = [
                    '\u00A0\u00A0\u00A0',
                    '.\u00A0\u00A0',
                    '..\u00A0',
                    '...'
                ];
                let dotIndex = 0;
                this._outlookTimer = setInterval(() => {
                    dotIndex = (dotIndex + 1) % dotStates.length;
                    this.outlookDots = dotStates[dotIndex];
                }, 500);

                this._outlookStopTimer = setTimeout(() => {
                    this.outlookOpening = false;
                    this.outlookDots = '';
                    if (this._outlookTimer) clearInterval(this._outlookTimer);
                    this._outlookTimer = null;
                    this._outlookStopTimer = null;
                }, 5000);
            },
            async toggleFavorite(item) {
                if (!item) return;
                try {
                    await toggleFavoriteApi(item.id);
                    item.isFavorite = !item.isFavorite;
                } catch (err) {
                    console.error('Failed to toggle favorite', err);
                }
            },

            async openResource(item) {
                try {
                    const api = await import('../services/api');
                    await api.recordOpen(item.id);
                } catch (err) {
                    console.error('record open failed', err);
                }
                window.open(item.url, '_blank', 'noopener');
            },
            shorten(s, max = 30) {
                if (!s || typeof s !== 'string') return '';
                return s.length > max ? s.slice(0, max) + '...' : s;
            },
            async copyUrl(url) {
                if (!url) return;
                try {
                    if (navigator.clipboard && navigator.clipboard.writeText) {
                        await navigator.clipboard.writeText(url);
                    } else {
                        const ta = document.createElement('textarea');
                        ta.value = url;
                        ta.style.position = 'fixed';
                        ta.style.opacity = '0';
                        document.body.appendChild(ta);
                        ta.select();
                        document.execCommand('copy');
                        document.body.removeChild(ta);
                    }
                } catch (e) {
                    console.error('copy failed', e);
                }
            }
        }
    };
</script>


<style scoped>
    .modal-backdrop {
        position: fixed;
        top: 0;
        left: 0;
        width: 100vw;
        height: 100vh;
        background-color: rgba(0,0,0,0.5);
        display: flex;
        justify-content: center;
        align-items: center;
        z-index: 1050;
    }

    .modal-content {
        position: relative;
        background-color: white;
        border-radius: 8px;
        padding: 0;
        max-width: 1320px;
        width: 100%;
    }

    .modal-header.custom-header {
        background-color: #f0f4f8;
        padding: 1rem 1.5rem;
        border-bottom: 1px solid #dee2e6;
        display: flex;
        justify-content: space-between;
        align-items: center;
        border-top-left-radius: 8px;
        border-top-right-radius: 8px;
    }

    .modal-body { padding: 1.5rem; max-height: 75vh; overflow: auto; }

    .scroll-hint {
        position: absolute;
        left: 50%;
        bottom: 0.9rem;
        transform: translateX(-50%);
        pointer-events: none;
        z-index: 3;
        color: #1aa6a6;
        font-size: 2.25rem;
        opacity: 0;
        filter: drop-shadow(0 3px 6px rgba(0,0,0,0.20));
        animation: scrollHintBlink 1s ease-in-out 0s 4;
        animation-fill-mode: both;
    }

    @keyframes scrollHintBlink {
        0% { opacity: 0; transform: translateX(-50%) translateY(0); }
        50% { opacity: 1; transform: translateX(-50%) translateY(-6px); }
        100% { opacity: 0; transform: translateX(-50%) translateY(0); }
    }

    .details-grid {
        display: grid;
        grid-template-columns: max-content 1fr max-content 1fr;
        row-gap: 0.8rem;
        column-gap: 1rem;
    }

    /* Make value cells align items consistently (center vertically) so multi-line
       content like tag chips aligns the same as other property values. */
    .details-grid > div {
        display: flex;
        align-items: center;
    }

    .details-grid > .align-top {
        align-items: flex-start;
    }

    .tags-value {
        grid-column: 2 / span 3;
    }

    .tags-label {
        grid-column: 1;
    }

    .info-icon {
        color: #2a7bd6;
        font-size: 0.85rem;
        margin-right: 0.35rem;
        opacity: 0.9;
        cursor: help;
        flex: 0 0 auto;
    }

    /* Instant tooltip (faster than native title tooltip) */
    .info-icon-wrap {
        position: relative;
        display: inline-flex;
        align-items: center;
        outline: none;
    }

    .info-icon-wrap[data-tooltip=""]::before,
    .info-icon-wrap[data-tooltip=""]::after {
        display: none;
    }

    .info-icon-wrap::after {
        content: attr(data-tooltip);
        position: absolute;
        left: 0;
        top: 1.35rem;
        z-index: 1000;
        max-width: 420px;
        width: max-content;
        padding: 0.5rem 0.6rem;
        border-radius: 0.45rem;
        background: #f2f7ff;
        border: 1px solid #cfe0ff;
        color: #0b2b4a;
        font-size: 0.85rem;
        line-height: 1.25;
        white-space: pre-line;
        box-shadow: 0 8px 18px rgba(0, 0, 0, 0.12);
        opacity: 0;
        transform: translateY(-2px);
        pointer-events: none;
        transition: opacity 80ms ease-out, transform 80ms ease-out;
    }

    .info-icon-wrap::before {
        content: "";
        position: absolute;
        left: 0.35rem;
        top: 1.12rem;
        border-width: 0 7px 7px 7px;
        border-style: solid;
        border-color: transparent transparent #cfe0ff transparent;
        opacity: 0;
        transform: translateY(-2px);
        pointer-events: none;
        transition: opacity 80ms ease-out, transform 80ms ease-out;
    }

    .info-icon-wrap:hover::after,
    .info-icon-wrap:hover::before,
    .info-icon-wrap:focus::after,
    .info-icon-wrap:focus::before {
        opacity: 1;
        transform: translateY(0);
    }

    /* Hover highlight for each field/value pair */
    .details-grid > .label,
    .details-grid > .label + div {
        padding: 0.15rem 0.35rem;
        border-radius: 0.35rem;
        transition: background-color 120ms ease-in-out;
    }

    .details-grid > .label:hover,
    .details-grid > .label:hover + div,
    .details-grid > div:not(.label):hover {
        background-color: #e6f7f7;
    }

    /* When hovering the value, also highlight its label (requires :has support) */
    @supports selector(:has(+ *)) {
        .details-grid > .label:has(+ div:hover) {
            background-color: #e6f7f7;
        }
    }

    .label {
        font-weight: 600;
        white-space: nowrap;
    }

    .flags { display: flex; gap: 0.4rem; align-items: center; }
    .url-row { display: inline-flex; align-items: center; gap: 0.25rem; }
    .copy-btn { color: #0d6efd; }
    .copy-btn:hover { color: #0a58ca; }

    /* Two-line clamp with ellipsis and fixed height */
    .clamp-2 {
        display: -webkit-box;
        -webkit-line-clamp: 2;
        -webkit-box-orient: vertical;
        overflow: hidden;
        white-space: normal;
        line-height: 1.35;
        max-height: 60px;
        min-height: calc(1.35em * 2);
    }
    .desc { max-width: 100%; }
    .req { color: #d9534f; }
    .modal-wide { width: 97vw; }

    /* Description cell should span remaining columns and align to top for multi-line */
    .desc-cell { grid-column: 2 / span 3; align-items: flex-start; }

    /* Description: default to 1 line when it fits; otherwise wrap and expand fully */
    .desc-auto {
        white-space: normal;
        overflow: visible;
    }

    .desc-fit-one {
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
    }

    /* Temporary measurement state (nowrap) */
    .desc-measure {
        white-space: nowrap;
        overflow: hidden;
    }

    .tag-list {
        display: flex;
        flex-wrap: wrap;
        gap: 0.375rem; /* 25% less than 0.5rem */
        margin: 0; /* ensure no extra vertical spacing */
        align-items: center;
    }

    .tag-chip {
        display: inline-flex;
        align-items: center;
        gap: 0.5rem;
        padding: 0.25rem 0.5rem;
        border-radius: 0.35rem;
        background-color: #eaf6ff;
        border: 1px solid #c7e6ff;
        color: #05567a;
        font-size: 0.85rem;
    }

    .tag-chip .tag-text {
        line-height: 1;
        display: inline-block;
        vertical-align: middle;
    }

    /* Styled like your existing favorite icon */
    .favorite-icon-btn {
        color: #ffad44c7;
        border-color: #ffad44c7;
        background: transparent;
    }

    .favorite-icon-btn:hover {
            color: #d78418c7;
            border-color: #d78418c7;
        }

    .favorite-toggle-btn {
        color: #ffad44c7;
        border: 1px solid transparent;
        background: transparent;
        padding: 0.1rem 0.35rem;
        line-height: 1;
    }

    .favorite-toggle-btn:hover {
        color: #d78418c7;
        background: #fff7ea;
        border-color: #ffad44c7;
    }

    .btn-outline-teal {
        border-color: #00A89E;
        color: #00A89E;
    }
    .btn-outline-teal:hover {
        background-color: #00A89E;
        color: #fff;
    }

    .action-btn {
        min-width: 140px;
    }

    .access-request-note {
        color: #5a6b7c;
        font-size: 0.85rem;
        align-self: center;
    }

    .request-access-overlay {
        position: absolute;
        inset: 0;
        background: rgba(255, 255, 255, 0.9);
        z-index: 5;
        display: flex;
        justify-content: center;
        align-items: flex-start;
        padding: 1.5rem;
    }

    .request-access-card {
        width: min(720px, 95%);
        background: #fff;
        border: 1px solid #dee2e6;
        border-radius: 8px;
        padding: 1rem;
        box-shadow: 0 10px 24px rgba(0,0,0,0.12);
    }

    .access-row {
        display: grid;
        grid-template-columns: 90px 1fr;
        gap: 0.5rem;
        align-items: center;
        margin-bottom: 0.35rem;
    }

    .access-label {
        margin-bottom: 0;
        font-weight: 600;
        text-align: right;
    }

    .access-body {
        min-height: 120px;
        resize: vertical;
    }

    .access-row-body {
        align-items: flex-start;
    }

    .access-users {
        min-height: 120px;
        resize: vertical;
    }

    .access-row-users {
        align-items: flex-start;
    }

    .access-required-note {
        font-size: 0.85rem;
    }

</style>
