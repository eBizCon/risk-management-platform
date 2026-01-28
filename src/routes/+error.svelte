<script lang="ts">
  let { error: _error, status } = $props();

  const title = $derived(
    status === 401
      ? 'Login erforderlich'
      : status === 403
        ? 'Keine Berechtigung'
        : 'Etwas ist schiefgelaufen'
  );

  const description = $derived(
    status === 401
      ? 'Bitte melden Sie sich an, um fortzufahren.'
      : status === 403
        ? 'Sie haben keine Berechtigung für diese Seite.'
        : 'Bitte versuchen Sie es erneut oder kehren Sie zur Startseite zurück.'
  );

  const errorTestId = $derived(
    status === 401
      ? 'auth-error-unauthorized'
      : status === 403
        ? 'auth-error-forbidden'
        : undefined
  );
</script>

<main class="min-h-screen flex items-center justify-center px-4">
  <div class="max-w-lg w-full bg-white shadow-md border border-default rounded-lg p-8 text-center">
    <p class="text-sm font-semibold text-secondary uppercase tracking-wide mb-2">Fehler {status}</p>
    <h1
      class="text-2xl font-bold text-primary mb-3"
      data-testid={errorTestId}
    >
      {title}
    </h1>
    <p class="text-base text-secondary mb-6">{description}</p>

    <div class="flex flex-col sm:flex-row gap-3 justify-center items-center">
      <a href="/login" class="btn-primary px-5 py-2" data-testid="auth-error-login-link">Zum Login</a>
      <a href="/" class="text-primary hover:underline">Zur Startseite</a>
    </div>
  </div>
</main>
