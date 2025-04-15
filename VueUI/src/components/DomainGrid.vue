
    <template>
        <div class="container my-5">
            <div class="d-flex justify-content-between align-items-center mb-4">
                <h2 class="text-dark">Browse by Domain</h2>
                <a href="#" class="text-teal">Explore All <i class="bi bi-arrow-right"></i></a>
            </div>
            <div class="row row-cols-1 row-cols-md-4 g-4">
                <div class="col" v-for="(domain, index) in domains" :key="index">
                    <router-link :to="{ name: 'DomainItems', params: { domainName: domain.title.toLowerCase().replace(/ & /g, '-').replace(/ /g, '-') } }"
                                 class="text-decoration-none">
                        <DomainCard :title="domain.title"
                                    :description="domain.description"
                                    :show-asset-type="false"
                                    :show-request-access="false" />
                    </router-link>
                </div>
            </div>
        </div>
    </template>

    <script>
        import DomainCard from './DomainCard.vue';

        export default {
            name: 'DomainGrid',
            components: {
                DomainCard,
            },
            data() {
                return {
                    domains: [
                        {
                            title: 'Access to Care',
                            description: 'Access to Care domain provides information about and allows users to request access to data assets related to healthcare availability, affordability, accessibility, accommodation, coverage, timeliness and workforce performance while providing patient care.',
                        },
                        {
                            title: 'Clinical Operations',
                            description: 'Clinical Operations domain provides information about and allows users to request access to data assets related to achieving low cost and productivity targets, improved turnaround time, and reduction in variation leading to improved quality and efficiency.',
                        },
                        {
                            title: 'Finance',
                            description: 'Finance domain provides information about and allows users to request access to data assets related to budgeting, billing, financial performance and forecasting, and reporting informed investment decisions ensuring financial health of the organization.',
                        },
                        {
                            title: 'Patient Experience',
                            description: 'Patient Experience domain provides information about and allows users to request access to data assets used to measure ongoing work related to the enhancement of overall patient experience achieved through accessibility, convenience, and care coordination.',
                        },
                        {
                            title: 'People & Workforce',
                            description: 'People and Workforce domain provides information about and allows users to request access to data assets related to metrics, such as time spent with patients, cost per patient, patient and staff satisfaction, and the overall value and quality of care provided.',
                        },
                        {
                            title: 'Quality',
                            description: 'Quality domain provides information about and allows users to request access to data assets related to implementing quality assurance programs and adherence to regulations and standards, essential for maintaining organizational reputation.',
                        },
                        {
                            title: 'Revenue Cycle',
                            description: 'This domain covers information and metrics related to billing, payments, and revenue collection.',
                        },
                        {
                            title: 'Service Lines',
                            description: 'Service Lines domain provides information and allows users to request access to data assets related to multiple cross regional clinical departments serving common patient needs, to provide oversight, support and strategic development of these departments.',
                        },
                    ],
                };
            },
        };
    </script>

    <style scoped>
        h2 {
            font-size: 1.5rem;
            font-weight: 500;
        }

        .card {
            transition: transform 0.2s;
        }

            .card:hover {
                transform: scale(1.05);
                cursor: pointer;
            }
    </style>
