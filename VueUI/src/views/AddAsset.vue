<template>
    <div>
        <Navbar />
        <div class="container mt-4">
            <h2>Add New Asset</h2>
            <form @submit.prevent="submitAsset" class="add-asset-form">
                <!-- Title -->
                <div class="mb-3">
                    <label for="title" class="form-label">Title</label>
                    <input type="text"
                           class="form-control"
                           id="title"
                           v-model="newAsset.title"
                           required />
                </div>

                <!-- Description -->
                <div class="mb-3">
                    <label for="description" class="form-label">Description</label>
                    <textarea class="form-control"
                              id="description"
                              v-model="newAsset.description"
                              rows="3"
                              required></textarea>
                </div>

                <!-- URL -->
                <div class="mb-3">
                    <label for="url" class="form-label">URL</label>
                    <input type="url"
                           class="form-control"
                           id="url"
                           v-model="newAsset.url"
                           required />
                </div>

                <!-- Asset Types -->
                <div class="mb-3">
                    <label class="form-label">Asset Types</label>
                    <div class="form-check" v-for="type in assetTypeOptions" :key="type">
                        <input class="form-check-input"
                               type="checkbox"
                               :id="`asset-type-${type}`"
                               :value="type"
                               v-model="newAsset.assetTypes" />
                        <label class="form-check-label" :for="`asset-type-${type}`">
                            {{ type }}
                        </label>
                    </div>
                </div>

                <!-- Has Access -->
                <div class="mb-3">
                    <label class="form-label">Access</label>
                    <div class="form-check">
                        <input class="form-check-input"
                               type="checkbox"
                               id="hasAccess"
                               v-model="newAsset.hasAccess" />
                        <label class="form-check-label" for="hasAccess">
                            Has Access
                        </label>
                    </div>
                </div>

                <!-- Privacy (PHI) -->
                <div class="mb-3">
                    <label class="form-label">Privacy</label>
                    <div class="form-check">
                        <input class="form-check-input"
                               type="checkbox"
                               id="phi"
                               v-model="newAsset.privacy.phi" />
                        <label class="form-check-label" for="phi">
                            Contains PHI
                        </label>
                    </div>
                </div>

                <!-- Domain -->
                <div class="mb-3">
                    <label for="domain" class="form-label">Domain</label>
                    <select class="form-select"
                            id="domain"
                            v-model="newAsset.domain"
                            required>
                        <option disabled value="">Select a domain</option>
                        <option v-for="domain in domainOptions" :key="domain" :value="domain">
                            {{ domain }}
                        </option>
                    </select>
                </div>

                <!-- Division -->
                <div class="mb-3">
                    <label for="division" class="form-label">Division</label>
                    <select class="form-select"
                            id="division"
                            v-model="newAsset.division"
                            required>
                        <option disabled value="">Select a division</option>
                        <option v-for="division in divisionOptions" :key="division" :value="division">
                            {{ division }}
                        </option>
                    </select>
                </div>

                <!-- Service Line -->
                <div class="mb-3">
                    <label for="serviceLine" class="form-label">Service Line</label>
                    <select class="form-select"
                            id="serviceLine"
                            v-model="newAsset.serviceLine"
                            required>
                        <option disabled value="">Select a service line</option>
                        <option v-for="line in serviceLineOptions" :key="line" :value="line">
                            {{ line }}
                        </option>
                    </select>
                </div>

                <!-- Data Source -->
                <div class="mb-3">
                    <label for="dataSource" class="form-label">Data Source</label>
                    <select class="form-select"
                            id="dataSource"
                            v-model="newAsset.dataSource"
                            required>
                        <option disabled value="">Select a data source</option>
                        <option v-for="source in dataSourceOptions" :key="source" :value="source">
                            {{ source }}
                        </option>
                    </select>
                </div>

                <!-- Submit Button -->
                <button type="submit" class="btn btn-primary">Add Asset</button>
            </form>
        </div>
    </div>
</template>

<script>
import Navbar from '../components/Navbar.vue';
import itemsData from '../data/itemsData.js';

export default {
    name: 'AddAsset',
    components: { Navbar },
    data() {
        return {
            newAsset: {
                id: null,
                title: '',
                description: '',
                url: '',
                assetTypes: [],
                hasAccess: false,
                privacy: { phi: false },
                domain: '',
                division: '',
                serviceLine: '',
                dataSource: '',
            },
            assetTypeOptions: ['Dashboard', 'Application', 'Data Model', 'Report', 'Featured'],
            domainOptions: [
                'Access to Care',
                'Clinical Operations',
                'Finance',
                'Patient Experience',
                'People & Workforce',
                'Quality',
                'Revenue Cycle',
                'Service Lines',
            ],
            divisionOptions: [
                'Greater Central Valley',
                'Greater East Bay',
                'Greater Sacramento',
                'Greater San Francisco',
            ],
            serviceLineOptions: [
                'Behavioral Health',
                'Cardiology',
                'Hospital',
                'Oncology',
                'Primary Care',
                'Orthopedics',
            ],
            dataSourceOptions: ['Power BI', 'Epic', 'Tableau', 'Web-Based'],
        };
    },
    methods: {
        submitAsset() {
            // Generate a new ID (max ID + 1)
            const newId = Math.max(...itemsData.map(item => item.id)) + 1;

            // Create the new asset object
            const asset = {
                id: newId,
                title: this.newAsset.title,
                description: this.newAsset.description,
                url: this.newAsset.url,
                assetTypes: this.newAsset.assetTypes,
                hasAccess: this.newAsset.hasAccess,
                privacy: { phi: this.newAsset.privacy.phi },
                domain: this.newAsset.domain,
                division: this.newAsset.division,
                serviceLine: this.newAsset.serviceLine,
                dataSource: this.newAsset.dataSource,
            };

            // Add the new asset to itemsData
            itemsData.push(asset);

            // Redirect back to the domain page (or home if no domain context)
            this.$router.push('/domain/access-to-care'); // Adjust based on desired redirect
        },
    },
};
</script>

<style scoped>
    .add-asset-form {
        max-width: 600px;
        margin: 0 auto;
    }

    .form-label {
        font-weight: 500;
    }

    .form-check {
        margin-bottom: 5px;
    }

    .btn-primary {
        background-color: #007bff;
        border-color: #007bff;
    }

        .btn-primary:hover {
            background-color: #0056b3;
            border-color: #004085;
        }
</style>