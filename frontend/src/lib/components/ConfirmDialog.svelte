<script lang="ts">
  interface Props {
    open: boolean;
    title?: string;
    message: string;
    confirmText?: string;
    cancelText?: string;
    onConfirm: () => void;
    onCancel: () => void;
  }

  let {
    open,
    title,
    message,
    confirmText = 'Bestätigen',
    cancelText = 'Abbrechen',
    onConfirm,
    onCancel
  }: Props = $props();

  const confirmLabel = $derived(confirmText ?? 'Bestätigen');
  const cancelLabel = $derived(cancelText ?? 'Abbrechen');

  let dialogRef = $state<HTMLDivElement | null>(null);
  let confirmButtonRef = $state<HTMLButtonElement | null>(null);

  function getFocusableElements(): HTMLElement[] {
    if (!dialogRef) return [];
    const candidates = dialogRef.querySelectorAll<HTMLElement>(
      'button, [href], input, select, textarea, [tabindex]:not([tabindex="-1"])'
    );
    return Array.from(candidates).filter((el) => !el.hasAttribute('disabled'));
  }

  function handleKeydown(event: KeyboardEvent) {
    if (event.key === 'Escape') {
      event.preventDefault();
      onCancel();
      return;
    }

    if (event.key === 'Enter' && event.target === dialogRef) {
      event.preventDefault();
      onConfirm();
      return;
    }

    if (event.key !== 'Tab') return;

    const focusable = getFocusableElements();
    if (focusable.length === 0) return;

    const first = focusable[0];
    const last = focusable[focusable.length - 1];
    const active = document.activeElement as HTMLElement | null;

    if (!event.shiftKey && active === last) {
      event.preventDefault();
      first.focus();
    } else if (event.shiftKey && active === first) {
      event.preventDefault();
      last.focus();
    }
  }

  function handleContentClick(event: MouseEvent) {
    event.stopPropagation();
  }

  function handleBackdropClick() {
    onCancel();
  }

  function handleBackdropKeydown(event: KeyboardEvent) {
    if (event.key === 'Enter' || event.key === ' ') {
      event.preventDefault();
      onCancel();
    }
  }

  $effect(() => {
    if (typeof document === 'undefined') return;
    const originalOverflow = document.body.style.overflow;
    if (open) {
      document.body.style.overflow = 'hidden';
      return () => {
        document.body.style.overflow = originalOverflow;
      };
    }
  });

  $effect(() => {
    if (open && confirmButtonRef) {
      confirmButtonRef.focus();
    }
  });
</script>

{#if open}
  <div
    class="fixed inset-0 z-50 flex items-center justify-center p-4"
    aria-hidden="false"
  >
    <div
      class="absolute inset-0 bg-black/50"
      role="button"
      aria-label="Dialog abbrechen"
      tabindex="0"
      onclick={handleBackdropClick}
      onkeydown={handleBackdropKeydown}
    ></div>
    <div
      class="relative z-10 card p-6 max-w-md w-full shadow-xl"
      role="dialog"
      aria-modal="true"
      aria-label={title ?? 'Bestätigung erforderlich'}
      tabindex="-1"
      onclick={handleContentClick}
      onkeydown={handleKeydown}
      data-testid="confirm-dialog"
      bind:this={dialogRef}
    >
      {#if title}
        <h2 class="text-xl font-semibold text-primary mb-2">{title}</h2>
      {/if}
      <p class="text-primary mb-6">{message}</p>
      <div class="flex justify-end gap-3">
        <button
          type="button"
          class="btn-secondary"
          onclick={onCancel}
          data-testid="confirm-dialog-cancel"
        >
          {cancelLabel}
        </button>
        <button
          type="button"
          class="btn-primary"
          onclick={onConfirm}
          data-testid="confirm-dialog-confirm"
          bind:this={confirmButtonRef}
        >
          {confirmLabel}
        </button>
      </div>
    </div>
  </div>
{/if}
