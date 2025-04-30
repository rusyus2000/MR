<template>
    <div>
        <Navbar />
        <div class="container my-5 mb-5">
            <div class="mx-auto" style="max-width: 60%;">
                <div class="card shadow-custom">
                    <div class="card-header">
                        <h3>Add New Asset</h3>
                    </div>
                    <div class="card-body">
                        <form @submit.prevent="submitForm">
                            <!-- Full-width fields -->
                            <div class="mb-3">
                                <label for="title" class="form-label fw-bold">Title</label>
                                <input v-model="form.title" type="text" id="title" class="form-control" required />
                            </div>
                            <div class="mb-3">
                                <label for="description" class="form-label fw-bold">Description</label>
                                <textarea v-model="form.description" id="description" class="form-control" rows="3" required></textarea>
                            </div>
                            <div class="mb-3">
                                <label for="url" class="form-label fw-bold">Resource URL</label>
                                <input v-model="form.url" type="url" id="url" class="form-control" required />
                            </div>

                            <!-- Two-column grid -->
                            <div class="row row-custom">
                                <!-- Asset Types spans two rows -->
                                <div class="col-md-6 asset-span-2 mb-3">
                                    <label for="assetTypes" class="form-label fw-bold">Asset Types</label>
                                    <select v-model="form.assetTypes" id="assetTypes" class="form-select w-100 asset-select" multiple>
                                        <option>Dashboard</option>
                                        <option>Application</option>
                                        <option>Data Model</option>
                                        <option>Report</option>
                                        <option>Featured</option>
                                    </select>
                                </div>
                                <!-- Domain -->
                                <div class="col-md-6 mb-3">
                                    <label for="domain" class="form-label fw-bold">Domain</label>
                                    <select v-model="form.domain" id="domain" class="form-select" required>
                                        <option value="">Select domain</option>
                                        <option>Strategy</option>
                                        <option>Access to Care</option>
                                        <option>Quality</option>
                                        <option>People & Workforce</option>
                                    </select>
                                </div>
                                <!-- Division -->
                                <div class="col-md-6 mb-3">
                                    <label for="division" class="form-label fw-bold">Division</label>
                                    <select v-model="form.division" id="division" class="form-select">
                                        <option value="">Select division</option>
                                        <option>Greater Central Valley</option>
                                        <option>Greater East Bay</option>
                                        <option>Greater Sacramento</option>
                                        <option>Greater San Francisco</option>
                                    </select>
                                </div>
                                <!-- Service Line -->
                                <div class="col-md-6 mb-3">
                                    <label for="serviceLine" class="form-label fw-bold">Service Line</label>
                                    <select v-model="form.serviceLine" id="serviceLine" class="form-select">
                                        <option value="">Select service line</option>
                                        <option>Behavioral Health</option>
                                        <option>Cardiology</option>
                                        <option>Hospital</option>
                                        <option>Oncology</option>
                                        <option>Primary Care</option>
                                        <option>Orthopedics</option>
                                    </select>
                                </div>
                                <!-- Data Source -->
                                <div class="col-md-6 mb-3">
                                    <label for="dataSource" class="form-label fw-bold">Data Source</label>
                                    <select v-model="form.dataSource" id="dataSource" class="form-select">
                                        <option value="">Select data source</option>
                                        <option>Power BI</option>
                                        <option>Epic</option>
                                        <option>Tableau</option>
                                        <option>Web-Based</option>
                                    </select>
                                </div>
                            </div>

                            <!-- Actions -->
                            <div class="d-flex justify-content-between mt-3">
                                <button type="button" class="btn btn-secondary" @click="$router.back()">Cancel</button>
                                <button type="submit" class="btn btn-primary">Save Asset</button>
                            </div>
                        </form>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script>
    import Navbar from '../components/Navbar.vue';
    export default {
        name: 'AddAsset',
        components: { Navbar },
        data() {
            return {
                form: {
                    title: '',
                    description: '',
                    url: '',
                    assetTypes: [],
                    domain: '',
                    division: '',
                    serviceLine: '',
                    dataSource: ''
                }
            };
        },
        methods: {
            submitForm() {
                console.log('Form submitted', this.form);
                this.$router.push({ name: 'Dashboard' });
            }
        }
    };
</script>

<style scoped>
    .asset-select {
        min-height: 9rem; /* taller combobox for asset types */
    }
    /* Ensure all form controls in the two-column grid span full width */
    .row-custom .form-select,
    .row-custom .form-control {
        width: 100%;
    }

    .row-custom > div {
        width: 100%; /* ensure full width in grid */
    }

    .shadow-custom {
        box-shadow: 0 10px 20px rgba(0, 0, 0, 0.15),0 6px 6px rgba(0, 0, 0, 0.1);
    }

    .card-header h3 {
        margin: 0;
    }

    .row-custom {
        display: grid;
        grid-template-columns: 1fr 1fr;
        grid-auto-rows: min-content;
        gap: 1rem;
    }

    .asset-span-2 {
        grid-row: span 2;
    }
</style>
