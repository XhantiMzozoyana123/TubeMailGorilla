import io
p = 'c:/Users/Shadow/Documents/TubeMailGorilla/TubeMailGorilla.Maui.Unlocked/Services/LLMService.cs'
lines = io.open(p, encoding='utf-8-sig').read().replace('\r\n', '\n').split('\n')
media_idx = [i for i, l in enumerate(lines) if 'mediaMarker' in l and 'Enumerable.Repeat' in l]
print('media lines:', [(i+1, lines[i].strip()[:60]) for i in media_idx])
# drop the two orphan copies that sit outside the images block (527 and 532 in 1-based)
for i in sorted(media_idx, reverse=True):
    del lines[i]
io.open(p, 'w', encoding='utf-8-sig', newline='').write('\n'.join(lines))
print('removed', len(media_idx))

