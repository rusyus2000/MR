<template>
  <div class="modal-backdrop" @click.self="close">
    <div class="modal-body">
      <div v-if="loading" class="overlay">
        <div class="overlay-card text-center">
          <div class="spinner-border text-primary mb-3" role="status" style="width: 3rem; height: 3rem;"></div>
          <div class="fw-semibold">{{ loadingMsg || 'Processing…' }}</div>
        </div>
      </div>
      <div class="d-flex justify-content-between align-items-center mb-2">
        <h5 class="mb-0">Upload Reports</h5>
        <button class="btn btn-sm btn-outline-secondary" @click="close">Close</button>
      </div>

      <div v-if="step===1">
        <div class="mb-3">
          <input type="file" accept=".csv" @change="onFile" />
        </div>
        <div class="mb-3 d-flex align-items-center">
          <label class="me-2">Mode</label>
          <select v-model="mode" class="form-select form-select-sm" style="width:auto;">
            <option value="upsert">Upsert</option>
          </select>
        </div>
        <button class="btn btn-primary" :disabled="!file || loading" @click="preview">{{ loading ? 'Processing…' : 'Preview' }}</button>
      </div>

      <div v-else-if="step===2">
        <div class="mb-3">
          <div class="fw-bold">Summary</div>
          <div class="small text-muted">Total: {{ summary.total }} | Create: {{ summary.toCreate }} | Update: {{ summary.toUpdate }} | Unchanged: {{ summary.unchanged }} | Conflicts: {{ summary.conflicts }} | Errors: {{ summary.errors }}</div>
        </div>
        <div v-if="errorsList.length" class="mb-4">
          <div class="fw-bold mb-1">Skipped (lookup errors)</div>
          <div class="errors-box p-2">
            <div v-for="e in errorsList" :key="e.index" class="small">Row {{ e.index }} — {{ e.reason }}</div>
          </div>
        </div>
        <div v-if="conflictsList.length" class="mb-4">
          <div class="fw-bold mb-2">Conflicts — choose Create, Skip, or Overwrite a match</div>
          <div v-for="c in conflictsList" :key="c.index" class="conflict-card">
            <div class="mb-2">
              <span class="badge bg-warning text-dark me-2">Row {{ c.index }}</span>
              <span class="small text-muted">{{ displayRow(c.row) }}</span>
            </div>
            <div class="conflict-actions d-flex align-items-center flex-wrap gap-3 mb-2">
              <label class="mb-0"><input type="radio" :name="'conf-'+c.index" value="create" v-model="ensureDecision(c.index).action"> Create New</label>
              <label class="mb-0"><input type="radio" :name="'conf-'+c.index" value="skip" v-model="ensureDecision(c.index).action"> Skip</label>
              <div class="d-flex align-items-center gap-2">
                <label class="mb-0"><input type="radio" :name="'conf-'+c.index" value="update" v-model="ensureDecision(c.index).action" @change="setUpdateDefault(c.index, c.candidates)"> Overwrite existing record</label>
                <select v-if="decisions[c.index] && decisions[c.index].action==='update'"
                        class="form-select form-select-sm d-inline-block"
                        style="width:auto; min-width: 360px;"
                        v-model="decisions[c.index].targetId">
                  <option v-for="cand in c.candidates" :key="(cand.id||cand.Id)" :value="(cand.id||cand.Id)">
                    {{ formatCand(cand) }}
                  </option>
                </select>
              </div>
            </div>
            <div v-if="decisions[c.index] && decisions[c.index].action==='update' && decisions[c.index].targetId" class="existing-box p-2">
              <div class="fw-bold mb-1">Selected existing record</div>
              <div class="small">
                {{ formatCand(findCandidate(c, decisions[c.index].targetId)) }}
              </div>
            </div>
          </div>
        </div>
        <div class="d-flex gap-2 justify-content-end">
          <button class="btn btn-outline-secondary" @click="reset">Start Over</button>
          <button class="btn btn-primary" :disabled="loading || !token" @click="commit">{{ loading ? 'Committing…' : 'Commit' }}</button>
        </div>
      </div>

      <div v-else-if="step===3">
        <div class="mb-2"><strong>Import Complete</strong></div>
        <div class="small text-muted mb-2">Created: {{ result.summary.created }}, Updated: {{ result.summary.updated }}, Unchanged: {{ result.summary.unchanged }}, Skipped: {{ result.summary.skipped }}</div>
        <div v-if="result.notImported && result.notImported.length">
          <strong>Not Imported</strong>
          <ul class="small mb-0">
            <li v-for="r in result.notImported" :key="r.index">Row {{ r.index }} — {{ r.reason }}</li>
          </ul>
        </div>
        <div class="mt-3">
          <button class="btn btn-primary" @click="close">Close</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script>
import { ref, reactive } from 'vue';
import { importPreview, importCommit, importRebuild } from '../services/api';

export default {
  name: 'ModalImport',
  emits: ['close','done'],
  setup(_, { emit }) {
    const step = ref(1);
    const file = ref(null);
    const loading = ref(false);
    const loadingMsg = ref('');
    const token = ref('');
    const mode = ref('upsert');
    const summary = reactive({ total: 0, toCreate: 0, toUpdate: 0, unchanged: 0, conflicts: 0, errors: 0 });
    const conflictsList = ref([]);
    const errorsList = ref([]);
    const decisions = reactive({}); // map: rowIndex -> { action, targetId? }
    const result = reactive({ summary: { created: 0, updated: 0, unchanged: 0, skipped: 0 }, notImported: [] });
    const formatCand = (cand) => {
      const id = cand.id || cand.Id;
      const t = cand.title || cand.Title || '';
      const u = cand.url || cand.Url || '';
      const d = cand.domain || cand.Domain || '';
      const a = cand.assetType || cand.AssetType || '';
      return `#${id} — ${t} (${u}, ${d}, ${a})`;
    };
    const findCandidate = (conflict, id) => {
      if (!conflict || !conflict.candidates) return null;
      const list = conflict.candidates;
      return list.find(x => (x.id||x.Id) === id) || null;
    };

    const onFile = (e) => {
      file.value = e.target.files && e.target.files[0] ? e.target.files[0] : null;
    };

    const preview = async () => {
      if (!file.value) return;
      loading.value = true;
      loadingMsg.value = 'Analyzing file…';
      try {
        const r = await importPreview(file.value, { mode: mode.value });
        token.value = r.token;
        summary.total = r.total;
        summary.toCreate = r.toCreate;
        summary.toUpdate = r.toUpdate;
        summary.unchanged = r.unchanged;
        summary.conflicts = r.conflicts;
        summary.errors = r.errors;
        conflictsList.value = r.conflictsList || [];
        errorsList.value = r.errorsList || [];
        // init decisions
        (conflictsList.value || []).forEach(c => { decisions[c.index] = { action: 'create', targetId: null }; });
        step.value = 2;
      } catch (e) {
        alert(e.message || 'Preview failed');
      } finally {
        loading.value = false;
        loadingMsg.value = '';
      }
    };

    const setUpdate = (idx, id) => {
      decisions[idx] = { action: 'update', targetId: id };
    };

    const displayRow = (row) => {
      if (!row) return '';
      const t = row.title || row.Title || '';
      const u = row.url || row.Url || '';
      const d = row.domain || row.Domain || '';
      const a = row.assetType || row.AssetType || '';
      return `${t} — (${u}, ${d}, ${a})`;
    };

    const ensureDecision = (idx) => {
      if (!decisions[idx]) decisions[idx] = { action: 'create', targetId: null };
      return decisions[idx];
    };

    const setUpdateDefault = (idx, candidates) => {
      const first = (candidates && candidates.length) ? (candidates[0].id || candidates[0].Id) : null;
      decisions[idx] = { action: 'update', targetId: first };
    };

    const commit = async () => {
      if (!token.value) return;
      loading.value = true;
      loadingMsg.value = 'Saving Records…';
      try {
        // Build decisions array
        const arr = Object.entries(decisions).map(([k, v]) => ({ rowIndex: parseInt(k, 10), action: v.action, targetId: v.targetId }));
        const r = await importCommit(token.value, arr);
        result.summary = r.summary || result.summary;
        result.notImported = r.notImported || [];
        // Phase 2: rebuild search index (server-side)
        loadingMsg.value = 'Rebuilding Search Index…';
        await importRebuild();
        step.value = 3;
        emit('done');
      } catch (e) {
        alert(e.message || 'Commit failed');
      } finally {
        loading.value = false;
        loadingMsg.value = '';
      }
    };

    const reset = () => {
      step.value = 1;
      file.value = null;
      token.value = '';
      conflictsList.value = [];
      errorsList.value = [];
      Object.keys(decisions).forEach(k => delete decisions[k]);
    };

    const close = () => emit('close');

    return { step, file, loading, loadingMsg, token, mode, summary, conflictsList, errorsList, decisions, result, onFile, preview, setUpdate, commit, reset, close, displayRow, ensureDecision, setUpdateDefault, formatCand, findCandidate };
  }
};
</script>

<style scoped>
.modal-backdrop {
  position: fixed;
  inset: 0;
  background: rgba(0,0,0,0.35);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 3000;
}
.modal-body {
  background: #fff;
  border-radius: 10px;
  padding: 20px;
  position: relative;
  width: 80vw;
  max-width: 1200px;
  max-height: 85vh;
  overflow: auto;
  box-shadow: 0 14px 40px rgba(0,0,0,0.25);
}
.overlay {
  position: absolute;
  inset: 0;
  background: rgba(255,255,255,0.85);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 10;
}
.overlay-card {
  background: #fff;
  border: 1px solid #e3e3e3;
  border-radius: 10px;
  padding: 24px 32px;
  box-shadow: 0 10px 30px rgba(0,0,0,0.15);
}
.conflict-card {
  border: 1px solid #e3e3e3;
  border-radius: 8px;
  padding: 12px;
  margin-bottom: 12px;
  background: #fafafa;
}
.errors-box {
  background: #fff7f7;
  border: 1px solid #ffd6d6;
  border-radius: 6px;
}
.existing-box {
  background: #f7fbff;
  border: 1px solid #d6ecff;
  border-radius: 6px;
}
</style>
