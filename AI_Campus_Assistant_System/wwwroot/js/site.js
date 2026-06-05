/* ═══════════════════════════════════════════════════════
   AI Campus — site.js  (shared utilities)
═══════════════════════════════════════════════════════ */

// ── Modal helpers ──────────────────────────────────────
function openModal(id)  { document.getElementById(id)?.classList.add('open'); }
function closeModal(id) { document.getElementById(id)?.classList.remove('open'); }

// Close modal on backdrop click
document.addEventListener('click', function(e) {
  if (e.target.classList.contains('modal-backdrop')) {
    e.target.classList.remove('open');
  }
});

// Close modal on ESC
document.addEventListener('keydown', function(e) {
  if (e.key === 'Escape') {
    document.querySelectorAll('.modal-backdrop.open')
      .forEach(m => m.classList.remove('open'));
  }
});

// ── Toast notifications ────────────────────────────────
function showToast(message, type = 'success') {
  const existing = document.getElementById('campus-toast');
  if (existing) existing.remove();

  const colors = {
    success: { bg: '#ECFDF5', border: '#A7F3D0', text: '#065F46', icon: 'bi-check-circle-fill' },
    error:   { bg: '#FFF1F2', border: '#FCA5A5', text: '#9F1239', icon: 'bi-exclamation-circle-fill' },
    info:    { bg: '#EFF6FF', border: '#BFDBFE', text: '#1E40AF', icon: 'bi-info-circle-fill' },
    warning: { bg: '#FFFBEB', border: '#FDE68A', text: '#92400E', icon: 'bi-exclamation-triangle-fill' },
  };
  const c = colors[type] || colors.info;

  const toast = document.createElement('div');
  toast.id = 'campus-toast';
  toast.style.cssText = `
    position:fixed;bottom:24px;right:24px;z-index:9999;
    background:${c.bg};border:1px solid ${c.border};color:${c.text};
    padding:13px 18px;border-radius:11px;font-size:13.5px;font-weight:600;
    display:flex;align-items:center;gap:9px;
    box-shadow:0 8px 24px rgba(0,0,0,.12);
    font-family:'Inter',sans-serif;
    animation:fadeInUp .3s ease;max-width:320px;
  `;
  toast.innerHTML = `<i class="bi ${c.icon}" style="font-size:16px;flex-shrink:0"></i>${message}`;
  document.body.appendChild(toast);
  setTimeout(() => toast.remove(), 3500);
}

// ── Confirm delete ─────────────────────────────────────
function confirmDelete(formId, name) {
  if (confirm(`Delete "${name}"? This cannot be undone.`)) {
    document.getElementById(formId)?.submit();
  }
}

// ── Password strength ──────────────────────────────────
function checkPasswordStrength(val, fillId, lblId) {
  let score = 0;
  if (val.length >= 6)  score++;
  if (val.length >= 10) score++;
  if (/[A-Z]/.test(val)) score++;
  if (/[0-9]/.test(val)) score++;
  if (/[^A-Za-z0-9]/.test(val)) score++;

  const levels = [
    { pct: '20%', color: '#EF4444', text: 'Too weak' },
    { pct: '40%', color: '#F97316', text: 'Weak' },
    { pct: '60%', color: '#EAB308', text: 'Fair' },
    { pct: '80%', color: '#22C55E', text: 'Strong' },
    { pct: '100%', color: '#10B981', text: 'Very strong' },
  ];
  const l = levels[Math.min(score, 4)];
  const fill = document.getElementById(fillId);
  const lbl  = document.getElementById(lblId);
  if (fill) { fill.style.width = l.pct; fill.style.background = l.color; }
  if (lbl)  { lbl.textContent = l.text; lbl.style.color = l.color; }
}

// ── Table search filter ────────────────────────────────
function filterTable(inputId, tableId) {
  const query = document.getElementById(inputId).value.toLowerCase();
  const rows  = document.querySelectorAll(`#${tableId} tbody tr`);
  rows.forEach(row => {
    row.style.display = row.textContent.toLowerCase().includes(query) ? '' : 'none';
  });
}

// ── Animate stat counters ──────────────────────────────
function animateCounters() {
  document.querySelectorAll('[data-count]').forEach(el => {
    const target = parseInt(el.dataset.count);
    let current  = 0;
    const step   = Math.ceil(target / 40);
    const timer  = setInterval(() => {
      current = Math.min(current + step, target);
      el.textContent = current.toLocaleString();
      if (current >= target) clearInterval(timer);
    }, 30);
  });
}

// ── Auto-hide alerts ───────────────────────────────────
document.addEventListener('DOMContentLoaded', () => {
  animateCounters();
  document.querySelectorAll('.alert').forEach(a => {
    setTimeout(() => {
      a.style.transition = 'opacity .5s';
      a.style.opacity    = '0';
      setTimeout(() => a.remove(), 500);
    }, 4000);
  });
});

// ── Timetable day order ────────────────────────────────
const DAY_ORDER = ['Monday','Tuesday','Wednesday','Thursday','Friday','Saturday','Sunday'];
