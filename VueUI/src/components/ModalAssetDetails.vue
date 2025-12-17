<template>
    <div class="modal-backdrop">
        <div class="modal-content shadow modal-wide">
            <div class="modal-header custom-header">
                <h5 class="modal-title d-flex align-items-center gap-2">
                    <i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Product Name *'] || ''"></i>
                    <span>{{ isLoading ? 'Loading…' : (item ? item.title : '') }}</span>
                    <button v-if="!isLoading && isAdmin && FEATURE_FLAGS.allowManualEdit" class="btn btn-link text-primary p-0 ms-2" title="Edit Item" @click="$emit('edit')">
                        <i class="bi bi-pencil-square fs-4"></i>
                    </button>
                    <button v-if="!isLoading && item" class="btn btn-sm favorite-toggle-btn" :title="item.isFavorite ? 'Remove from Favorites' : 'Add to Favorites'" @click="toggleFavorite(item)">
                        <i :class="item.isFavorite ? 'bi bi-star-fill' : 'bi bi-star'"></i>
                    </button>
                </h5>
                <div class="d-flex align-items-center gap-2">
                    <button class="btn-close" @click="$emit('close')"></button>
                </div>
            </div>
            <div class="modal-body" ref="modalBody">
                <div v-if="isLoading" class="d-flex justify-content-center align-items-center" style="min-height:160px;">
                    <div class="spinner-border text-primary" role="status">
                        <span class="visually-hidden">Loading...</span>
                    </div>
                </div>
                <div v-else class="details-grid">
                    <!-- Required fields block (top) -->
                    <!-- Row 1: Product Name removed (title shown in header) -->

                    <!-- Row 2: Description spans across -->
                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Product Description and Purpose *'] || ''"></i>Product Description and Purpose <span class="req">*</span>:</div>
                    <div class="desc desc-cell clamp-2" :title="(item.description && item.description.trim()) ? item.description : 'Missing Data'">
                        {{ (item.description && item.description.trim()) ? shorten(item.description, 240) : 'Missing Data' }}
                    </div>

                    <!-- Row 3: Location/URL | Division -->
                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Location/URL *'] || ''"></i>Location/URL <span class="req">*</span>:</div>
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
                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Division *'] || ''"></i>Division <span class="req">*</span>:</div><div>{{ item.division || 'Missing Data' }}</div>

                    <!-- Row 4: Domain | Operating Entity -->
                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Domain *'] || ''"></i>Domain <span class="req">*</span>:</div><div>{{ item.domain || 'Missing Data' }}</div>
                    <!-- Row 5: Operating Entity | Executive Sponsor -->
                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Operating Entity *'] || ''"></i>Operating Entity <span class="req">*</span>:</div><div>{{ item.operatingEntity || 'Missing Data' }}</div>
                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Executive Sponsor *'] || ''"></i>Executive Sponsor <span class="req">*</span>:</div>
                    <div>
                        <template v-if="item.executiveSponsorEmail">
                            <a :href="`mailto:${item.executiveSponsorEmail}`">{{ item.executiveSponsorName || item.executiveSponsorEmail }}</a>
                        </template>
                        <template v-else>
                            {{ item.executiveSponsorName || 'Missing Data' }}
                        </template>
                    </div>

                    <!-- Row 6: D&A Product Owner | Data Consumers -->
                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['D&A Product Owner *'] || ''"></i>D&amp;A Product Owner <span class="req">*</span>:</div>
                    <div>
                        <template v-if="item.ownerEmail">
                            <a :href="`mailto:${item.ownerEmail}`">{{ item.ownerName || item.ownerEmail }}</a>
                        </template>
                        <template v-else>
                            {{ item.ownerName || 'Missing Data' }}
                        </template>
                    </div>
                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Data Consumers *'] || ''"></i>Data Consumers <span class="req">*</span>:</div>
                    <div><span v-if="item.dataConsumers && item.dataConsumers.trim()">{{ item.dataConsumers }}</span><span v-else class="text-muted">Missing Data</span></div>

                    <!-- Row 7: BI Platform | Dependencies -->
                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['BI Platform *'] || ''"></i>BI Platform <span class="req">*</span>:</div><div>{{ item.biPlatform || 'Missing Data' }}</div>
                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Dependencies'] || ''"></i>Dependencies <span class="req">*</span>:</div>
                    <div><span v-if="item.dependencies && item.dependencies.trim()">{{ item.dependencies }}</span><span v-else class="text-muted">Missing Data</span></div>

                    <!-- Row 8: Default AD Groups | Flags -->
                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Default AD Group Names *'] || ''"></i>Default AD Groups <span class="req">*</span>:</div>
                    <div><span v-if="item.defaultAdGroupNames && item.defaultAdGroupNames.trim()">{{ item.defaultAdGroupNames }}</span><span v-else class="text-muted">Missing Data</span></div>
                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Flags'] || ''"></i>Flags <span class="req">*</span>:</div>
                    <div class="flags">
                        <span>PHI: {{ item.privacyPhiDisplay || (item.privacyPhi === null || item.privacyPhi === undefined ? 'Missing Data' : (item.privacyPhi ? 'Yes' : 'No')) }}</span>
                        <span>PII: {{ item.privacyPiiDisplay || (item.privacyPii === null || item.privacyPii === undefined ? 'Missing Data' : (item.privacyPii ? 'Yes' : 'No')) }}</span>
                        <span>RLS: {{ item.hasRlsDisplay || (item.hasRls === null || item.hasRls === undefined ? 'Missing Data' : (item.hasRls ? 'Yes' : 'No')) }}</span>
                    </div>

                    <!-- Row 9: Refresh Frequency | Last Date Modified -->
                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Refresh Frequency *'] || ''"></i>Refresh Frequency <span class="req">*</span>:</div><div>{{ item.refreshFrequency || 'Missing Data' }}</div>
                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Last Date Modified for this row *'] || ''"></i>Last Date Modified <span class="req">*</span>:</div>
                    <div>{{ item.lastModifiedDate ? new Date(item.lastModifiedDate).toLocaleDateString() : 'Missing Data' }}</div>

                    <!-- End required block -->

                    <!-- Remaining fields -->
                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Asset Type *'] || ''"></i>Asset Type:</div>
                    <div>{{ item.assetTypeName }}</div>
                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Product Status'] || ''"></i>Status:</div>
                    <div>{{ item.status || '-' }}</div>

                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Data Source *'] || ''"></i>Data Source:</div>
                    <div>{{ item.dataSource }}</div>
                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Product Status Notes'] || ''"></i>Product Status Notes:</div>
                    <div>{{ item.productStatusNotes || '-' }}</div>

                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Potential to Consolidate'] || ''"></i>Potential to Consolidate:</div>
                    <div>{{ item.potentialToConsolidate || '-' }}</div>
                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Product Group'] || ''"></i>Product Group:</div>
                    <div>{{ item.productGroup || '-' }}</div>

                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Potential to Automate'] || ''"></i>Potential to Automate:</div>
                    <div>{{ item.potentialToAutomate || '-' }}</div>
                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Product Impact Category'] || ''"></i>Product Impact Category:</div>
                    <div>{{ item.productImpactCategory || '-' }}</div>

                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Estimated 2025 development hours'] || ''"></i>Estimated Dev Hours:</div>
                    <div>{{ item.estimatedDevHours || '-' }}</div>
                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Tech Delivery Mgr'] || ''"></i>Tech Delivery Mgr:</div>
                    <div>{{ item.techDeliveryManager || '-' }}</div>

                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Business Value rated by the executive sponsor'] || ''"></i>Business Value:</div>
                    <div>{{ item.sponsorBusinessValue || '-' }}</div>
                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Regulatory/Compliance/Contractual Flag*'] || ''"></i>Regulatory/Compliance/Contractual:</div>
                    <div>{{ item.regulatoryComplianceContractual || '-' }}</div>

                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Development Effort'] || ''"></i>Development Effort:</div>
                    <div>{{ item.developmentEffort || '-' }}</div>
                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['DB Server'] || ''"></i>DB Server:</div>
                    <div>{{ item.dbServer || '-' }}</div>

                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Resources - Development'] || ''"></i>Resources - Development:</div>
                    <div>{{ item.resourcesDevelopment || '-' }}</div>
                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['DB/Data Mart'] || ''"></i>DB/Data Mart:</div>
                    <div>{{ item.dbDataMart || '-' }}</div>

                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Resources - Analysts'] || ''"></i>Resources - Analysts:</div>
                    <div>{{ item.resourcesAnalysts || '-' }}</div>
                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Database Table'] || ''"></i>Database Table:</div>
                    <div>{{ item.databaseTable || '-' }}</div>

                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Resources - Platform'] || ''"></i>Resources - Platform:</div>
                    <div>{{ item.resourcesPlatform || '-' }}</div>
                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Source Repo'] || ''"></i>Source Rep:</div>
                    <div>{{ item.sourceRep || '-' }}</div>

                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Resources - Data Engineering'] || ''"></i>Resources - Data Engineering:</div>
                    <div>{{ item.resourcesDataEngineering || '-' }}</div>
                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['dataSecurityClassification'] || ''"></i>Data Security Classification:</div>
                    <div>{{ item.dataSecurityClassification || '-' }}</div>

                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['accessGroupName'] || ''"></i>Access Group Name:</div>
                    <div>{{ item.accessGroupName || '-' }}</div>
                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['accessGroupDN'] || ''"></i>Access Group DN:</div>
                    <div>{{ item.accessGroupDn || '-' }}</div>

                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Automation Classification'] || ''"></i>Automation Classification:</div>
                    <div>{{ item.automationClassification || '-' }}</div>
                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['user_visibility_string'] || ''"></i>User Visibility (String):</div>
                    <div>{{ item.userVisibilityString || '-' }}</div>

                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Epic Security Group tag *'] || ''"></i>Epic Security Group Tag:</div>
                    <div>{{ item.epicSecurityGroupTag || '-' }}</div>
                    <div class="label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Keep'] || ''"></i>Keep Long Term:</div>
                    <div>{{ item.keepLongTerm || '-' }}</div>

                    <div class="label align-top tags-label"><i class="bi bi-info-circle info-icon" :title="FIELD_DEFINITIONS['Tags'] || ''"></i>Tags:</div>
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
            <div v-if="!isLoading && item" class="d-flex justify-content-end mt-3 px-4 pb-3">
                <button class="btn btn-sm btn-outline-secondary" @click="$emit('close')">Close</button>
            </div>
        </div>
    </div>
</template>

<script>
    import { toggleFavoriteApi } from '../services/api';
    import { FEATURE_FLAGS } from '../config';

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
            return { FEATURE_FLAGS, FIELD_DEFINITIONS, showScrollHint: false, _scrollHintTimer: null, _scrollHintSeq: 0 };
        },
        watch: {
            isLoading() {
                this.maybeShowScrollHint();
            },
            item() {
                this.maybeShowScrollHint();
            }
        },
        mounted() {
            this.maybeShowScrollHint();
        },
        beforeUnmount() {
            if (this._scrollHintTimer) clearTimeout(this._scrollHintTimer);
        },
        methods: {
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

</style>
