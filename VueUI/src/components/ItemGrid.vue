<template>
    <div class="item-grid">
        <div class="d-flex justify-content-between align-items-center mb-4">
            <span class="text-muted">Showing {{ paginatedItems.length }} of {{ filteredItems.length }} of {{ items.length }} results</span>
            <div class="d-flex align-items-center">
                <label for="sortBy" class="me-2">Sort by:</label>
                <select id="sortBy" class="form-select form-select-sm" v-model="sortBy">
                    <option>Most Relevant</option>
                    <option>Alphabetical</option>
                </select>
                <button class="btn btn-sm btn-outline-secondary ms-2">
                    <i class="bi bi-grid"></i>
                </button>
                <button class="btn btn-sm btn-outline-secondary ms-1">
                    <i class="bi bi-list"></i>
                </button>
            </div>
        </div>

        <div class="row row-cols-1 row-cols-md-3 g-4">
            <div class="col" v-for="(item, index) in paginatedItems" :key="index">
                <DomainCard :title="item.title"
                            :description="item.description"
                            :asset-types="item.assetTypes"
                            :show-asset-type="true"
                            :show-request-access="true"
                            :has-access="item.hasAccess" />
            </div>
        </div>

        <!-- Pagination -->
        <div class="d-flex justify-content-between align-items-center mt-4">
            <div class="d-flex align-items-center">
                <label for="itemsPerPage" class="me-2">Items per page</label>
                <select id="itemsPerPage" class="form-select form-select-sm" v-model="itemsPerPage" style="width: auto;">
                    <option>15</option>
                    <option>30</option>
                    <option>50</option>
                </select>
            </div>
            <div class="text-muted">
                {{ (currentPage - 1) * itemsPerPage + 1 }}-{{ Math.min(currentPage * itemsPerPage, filteredItems.length) }} of {{ filteredItems.length }}
            </div>
            <div>
                <button class="btn btn-sm btn-outline-secondary me-1" :disabled="currentPage === 1" @click="currentPage--">
                    <i class="bi bi-chevron-left"></i>
                </button>
                <button class="btn btn-sm btn-outline-secondary" :disabled="currentPage === totalPages" @click="currentPage++">
                    <i class="bi bi-chevron-right"></i>
                </button>
            </div>
        </div>
    </div>
</template>

<script>
    import DomainCard from './DomainCard.vue';

    export default {
        name: 'ItemGrid',
        components: {
            DomainCard,
        },
        props: {
            filters: {
                type: Object,
                default: () => ({
                    assetTypes: [],
                    privacy: { phi: false },
                    domains: [],
                    divisions: [],
                    serviceLines: [],
                    dataSources: [],
                }),
            },
        },
        data() {
            return {
                sortBy: 'Most Relevant',
                itemsPerPage: 15,
                currentPage: 1,
                items: [
                    {
                        title: 'Enterprise Refer Dashboard Monthly',
                        description: 'Comprehensive, self-service analytic application which provides insights into referrals, and to-from patterns within the Sutter network, strengthening community relationships. The application is expected to include known...',
                        assetTypes: ['Dashboard', 'Featured'],
                        hasAccess: false,
                        privacy: { phi: true },
                        domain: 'Access to Care',
                        division: 'Greater Central Valley',
                        serviceLine: 'Primary Care',
                        dataSource: 'Power BI',
                    },
                    {
                        title: 'Enterprise Refer Dashboard Daily',
                        description: 'Comprehensive, self-service analytic application which provides insights into referrals, and to-from patterns within the Sutter network, strengthening community relationships. The application is expected to surface known...',
                        assetTypes: ['Dashboard'],
                        hasAccess: true,
                        privacy: { phi: false },
                        domain: 'Access to Care',
                        division: 'Greater East Bay',
                        serviceLine: 'Cardiology',
                        dataSource: 'Epic',
                    },
                    {
                        title: 'Analytic Navigator',
                        description: 'Dashboards used by decision makers to optimize operational efficiency by improving resource allocation, reducing waste, and streamlining processes.',
                        assetTypes: ['Application'],
                        hasAccess: false,
                        privacy: { phi: false },
                        domain: 'Access to Care',
                        division: 'Greater Sacramento',
                        serviceLine: 'Hospital',
                        dataSource: 'Tableau',
                    },
                    {
                        title: 'Hospital Capacity',
                        description: 'This dashboard is the sole source for capacity information across Sutter Health. The dashboard is used for assessing capacity within hospitals, and includes capacity for the overall hospital, including a breakdown of capacity for the hospital, including a breakdown of capacity by bed, the current occupancy, and...',
                        assetTypes: ['Dashboard', 'Featured'],
                        hasAccess: true,
                        privacy: { phi: true },
                        domain: 'Access to Care',
                        division: 'Greater San Francisco',
                        serviceLine: 'Oncology',
                        dataSource: 'Power BI',
                    },
                    {
                        title: 'FLASH Report',
                        description: 'Targeted dashboard that includes the underlying FLASH Reports: Acute, Foundation, ASC, EPIC.',
                        assetTypes: ['Dashboard'],
                        hasAccess: false,
                        privacy: { phi: false },
                        domain: 'Access to Care',
                        division: 'Greater Central Valley',
                        serviceLine: 'Behavioral Health',
                        dataSource: 'Epic',
                    },
                    {
                        title: 'System Capacity',
                        description: 'This dashboard is the sole source for capacity information across Sutter Health. The dashboard is used by capacity managers and staff. This dashboard displays overall capacity for the entire Sutter system, at a high level. It includes key metrics for each of the locations, broken down...',
                        assetTypes: ['Dashboard'],
                        hasAccess: false,
                        privacy: { phi: true },
                        domain: 'Access to Care',
                        division: 'Greater East Bay',
                        serviceLine: 'Primary Care',
                        dataSource: 'Power BI',
                    },
                    {
                        title: 'Visits',
                        description: 'SutterView data models provide self-service data exploration tools. This data model contains a subset of ambulatory visits and related information. It does not include information about...',
                        assetTypes: ['Data Model'],
                        hasAccess: true,
                        privacy: { phi: false },
                        domain: 'Access to Care',
                        division: 'Greater Sacramento',
                        serviceLine: 'Cardiology',
                        dataSource: 'Web-Based',
                    },
                    {
                        title: 'FTE Flash Report - Weekly',
                        description: 'Targeted dashboards with metrics specific to FTE. Metrics cover all markets, entities, departments, divisions and specialties.',
                        assetTypes: ['Report'],
                        hasAccess: false,
                        privacy: { phi: false },
                        domain: 'Access to Care',
                        division: 'Greater San Francisco',
                        serviceLine: 'Hospital',
                        dataSource: 'Power BI',
                    },
                    {
                        title: 'ASC Flash Report - Monthly',
                        description: 'Targeted dashboards with metrics specific to ASC. Metrics cover all markets, entities, divisions and specialties.',
                        assetTypes: ['Report'],
                        hasAccess: true,
                        privacy: { phi: false },
                        domain: 'Access to Care',
                        division: 'Greater Central Valley',
                        serviceLine: 'Oncology',
                        dataSource: 'Tableau',
                    },
                    {
                        title: 'Detail Summary - MOR (Monthly Operating Review)',
                        description: 'Monthly operating review of the top metrics across all markets and entities with additional dimensions focusing on Need, Attention, Finance, Clinical Operations, Access to Care, People & Workforce, Growth, Quality and Physician Recruitment.',
                        assetTypes: ['Report', 'Featured'],
                        hasAccess: false,
                        privacy: { phi: false },
                        domain: 'Access to Care',
                        division: 'Greater East Bay',
                        serviceLine: 'Behavioral Health',
                        dataSource: 'Power BI',
                    },
                    {
                        title: 'Foundation Flash Report - Weekly',
                        description: 'Targeted dashboards with metrics specific to Foundation. Metrics cover all markets, entities, divisions and specialties.',
                        assetTypes: ['Report'],
                        hasAccess: false,
                        privacy: { phi: false },
                        domain: 'Access to Care',
                        division: 'Greater Sacramento',
                        serviceLine: 'Primary Care',
                        dataSource: 'Epic',
                    },
                    {
                        title: 'Foundation Flash Report - Monthly',
                        description: 'Targeted dashboards with metrics specific to Foundation. Metrics cover all markets, entities, divisions and specialties.',
                        assetTypes: ['Report'],
                        hasAccess: true,
                        privacy: { phi: false },
                        domain: 'Access to Care',
                        division: 'Greater San Francisco',
                        serviceLine: 'Cardiology',
                        dataSource: 'Power BI',
                    },
                    {
                        title: 'ASC Flash Report - Weekly',
                        description: 'Targeted dashboards with metrics specific to ASC. Metrics cover all markets, entities, divisions and specialties.',
                        assetTypes: ['Report'],
                        hasAccess: false,
                        privacy: { phi: false },
                        domain: 'Access to Care',
                        division: 'Greater Central Valley',
                        serviceLine: 'Hospital',
                        dataSource: 'Tableau',
                    },
                    {
                        title: 'Acute Flash Report - Weekly',
                        description: 'Targeted dashboards with metrics specific to Acute. Metrics cover all markets, entities, divisions and specialties.',
                        assetTypes: ['Report'],
                        hasAccess: true,
                        privacy: { phi: false },
                        domain: 'Access to Care',
                        division: 'Greater East Bay',
                        serviceLine: 'Oncology',
                        dataSource: 'Power BI',
                    },
                    {
                        title: 'Acute Flash Report - Monthly',
                        description: 'Targeted dashboards with metrics specific to Acute. Metrics cover all markets, entities, divisions and specialties.',
                        assetTypes: ['Report'],
                        hasAccess: false,
                        privacy: { phi: false },
                        domain: 'Access to Care',
                        division: 'Greater Sacramento',
                        serviceLine: 'Behavioral Health',
                        dataSource: 'Epic',
                    },
                    {
                        title: 'Sample Dashboard 1',
                        description: 'A sample dashboard for testing purposes.',
                        assetTypes: ['Dashboard'],
                        hasAccess: true,
                        privacy: { phi: false },
                        domain: 'Access to Care',
                        division: 'Greater San Francisco',
                        serviceLine: 'Primary Care',
                        dataSource: 'Power BI',
                    },
                    {
                        title: 'Sample Dashboard 2',
                        description: 'A sample dashboard for testing purposes.',
                        assetTypes: ['Dashboard'],
                        hasAccess: false,
                        privacy: { phi: false },
                        domain: 'Access to Care',
                        division: 'Greater Central Valley',
                        serviceLine: 'Cardiology',
                        dataSource: 'Tableau',
                    },
                    {
                        title: 'Sample Report 1',
                        description: 'A sample report for testing purposes.',
                        assetTypes: ['Report'],
                        hasAccess: true,
                        privacy: { phi: false },
                        domain: 'Access to Care',
                        division: 'Greater East Bay',
                        serviceLine: 'Hospital',
                        dataSource: 'Power BI',
                    },
                    {
                        title: 'Sample Report 2',
                        description: 'A sample report for testing purposes.',
                        assetTypes: ['Report'],
                        hasAccess: false,
                        privacy: { phi: false },
                        domain: 'Access to Care',
                        division: 'Greater Sacramento',
                        serviceLine: 'Oncology',
                        dataSource: 'Epic',
                    },
                    {
                        title: 'Sample Report 3',
                        description: 'A sample report for testing purposes.',
                        assetTypes: ['Report'],
                        hasAccess: true,
                        privacy: { phi: false },
                        domain: 'Access to Care',
                        division: 'Greater San Francisco',
                        serviceLine: 'Behavioral Health',
                        dataSource: 'Power BI',
                    },
                ],
            };
        },
        computed: {
            filteredItems() {
                return this.items.filter(item => {
                    // Asset Type Filter
                    if (this.filters.assetTypes.length > 0) {
                        const matchesAssetType = this.filters.assetTypes.some(type =>
                            item.assetTypes.includes(type)
                        );
                        if (!matchesAssetType) return false;
                    }

                    // Privacy Filter (PHI)
                    if (this.filters.privacy.phi) {
                        if (!item.privacy.phi) return false;
                    }

                    // Domain Filter
                    if (this.filters.domains.length > 0) {
                        if (!this.filters.domains.includes(item.domain)) return false;
                    }

                    // Division Filter
                    if (this.filters.divisions.length > 0) {
                        if (!this.filters.divisions.includes(item.division)) return false;
                    }

                    // Service Line Filter
                    if (this.filters.serviceLines.length > 0) {
                        if (!this.filters.serviceLines.includes(item.serviceLine)) return false;
                    }

                    // Data Source Filter
                    if (this.filters.dataSources.length > 0) {
                        if (!this.filters.dataSources.includes(item.dataSource)) return false;
                    }

                    return true;
                });
            },
            totalPages() {
                return Math.ceil(this.filteredItems.length / this.itemsPerPage);
            },
            paginatedItems() {
                const start = (this.currentPage - 1) * this.itemsPerPage;
                const end = start + this.itemsPerPage;
                let sortedItems = this.filteredItems;
                if (this.sortBy === 'Alphabetical') {
                    sortedItems = [...this.filteredItems].sort((a, b) => a.title.localeCompare(b.title));
                }
                return sortedItems.slice(start, end);
            },
        },
        watch: {
            filteredItems() {
                // Reset to the first page when filters change
                this.currentPage = 1;
            },
        },
    };
</script>

<style scoped>
    .item-grid {
        padding: 20px;
    }

    .form-select-sm {
        width: auto;
    }

    .btn-outline-secondary {
        border-color: #e0e0e0;
    }

    .text-muted {
        font-size: 0.9rem;
    }
</style>