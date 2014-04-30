// test only: delete if not used
function MinimizeRP(panelId, separatorImageId, collapseLeft) {
    var LPcell = document.getElementById(panelId);
    LPcell.style.display = 'none';
    UpdateSeparatorsImages(separatorImageId, collapseLeft, LPcell.style.display);
    _aspxSetCookie(panelId + postfix, LPcell.style.display);
    AdjustSize();
}