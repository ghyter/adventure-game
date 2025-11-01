export function enableDrag(el) {
    if (!el || el._dragHandlers) return;
    let dragging = false, lastX = 0, lastY = 0;
    function start(e) {
        if (e.button !== 0) return;
        if (e.target.closest('.scene')) return; // don't pan while dragging scene
        dragging = true;
        try { el.setPointerCapture(e.pointerId); } catch { }
        lastX = e.clientX; lastY = e.clientY;
        el.style.cursor = 'grabbing';
        e.preventDefault();
    }
    function move(e) {
        if (!dragging) return;
        const dx = e.clientX - lastX, dy = e.clientY - lastY;
        el.scrollLeft -= dx; el.scrollTop -= dy;
        lastX = e.clientX; lastY = e.clientY;
    }
    function end(e) {
        dragging = false;
        try { el.releasePointerCapture(e.pointerId); } catch { }
        el.style.cursor = 'grab';
    }
    el.addEventListener('pointerdown', start, { passive: false });
    el.addEventListener('pointermove', move);
    el.addEventListener('pointerup', end);
    el.addEventListener('pointercancel', end);
    el._dragHandlers = { start, move, end };
}

export function disableDrag(el) {
    if (!el || !el._dragHandlers) return;
    const { start, move, end } = el._dragHandlers;
    el.removeEventListener('pointerdown', start);
    el.removeEventListener('pointermove', move);
    el.removeEventListener('pointerup', end);
    el.removeEventListener('pointercancel', end);
    delete el._dragHandlers;
    el.style.cursor = '';
}
export function attachMapWheelListener(element, dotNetHelper) {
    if (!element) return;

    element.addEventListener(
        "wheel",
        (e) => {
            // Only intercept when Ctrl (or Cmd) is held
            if (e.ctrlKey || e.metaKey) {
                e.preventDefault();
                // deltaY < 0 => zoom in, > 0 => zoom out
                dotNetHelper.invokeMethodAsync("HandleMapWheel", e.deltaY);
            }
        },
        { passive: false } // IMPORTANT: allows preventDefault()
    );
}