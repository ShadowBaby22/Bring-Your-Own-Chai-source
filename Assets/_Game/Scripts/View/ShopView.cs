using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace KadakChai
{
    /// <summary>
    /// JhatPat, the shop app on the hero's phone (GDD: Weapons and the JhatPat shop).
    /// Today's stock (3 items + Cutting Chai) and the Mystery Box, whose ultra-rare drop is the Laathi.
    /// </summary>
    public class ShopView : MonoBehaviour
    {
        const float PW = 900, PH = 1520; // inner screen size

        Run run;
        Action onChanged;
        Action<string, CardStyle> log;
        RectTransform screen, list;
        Text balance, equipped;
        bool open, busy;

        public static readonly Color Brand = UiKit.Hex("#FFD23F");

        public static Color RarityColor(string r) => r switch
        {
            "ultra" => UiKit.Hex("#FFB300"),
            "epic" => UiKit.Hex("#AB47BC"),
            "rare" => UiKit.Hex("#42A5F5"),
            _ => UiKit.Hex("#9E9E9E"),
        };

        public static string RarityName(string r) => r switch { "ultra" => "ULTRA RARE", "epic" => "EPIC", "rare" => "RARE", _ => "COMMON" };

        static string TypeTag(ItemData it) => it.slot switch
        {
            "weapon" => it.type == "ranged" ? "RANGED" : "CLOSE",
            "armour" => "ARMOUR",
            "pass" => "PASS",
            "antidote" => "CURE",
            "insurance" => "INSURANCE",
            "consumable" => "CHAI",
            _ => "JUNK",
        };

        static Color TypeColor(ItemData it) => it.slot switch
        {
            "weapon" => it.type == "ranged" ? UiKit.Hex("#1E88E5") : UiKit.Hex("#E53935"),
            "armour" => UiKit.Hex("#546E7A"),
            "pass" => UiKit.Hex("#00897B"),
            "antidote" => UiKit.Hex("#43A047"),
            "insurance" => UiKit.Hex("#1565C0"),
            _ => UiKit.Hex("#8D6E63"),
        };

        public static IEnumerator Open(RectTransform phone, Run run, Action onChanged, Action<string, CardStyle> log)
        {
            var dim = UiKit.Panel(phone, "Shop", new Color(0, 0, 0, 0.8f));
            dim.rectTransform.Fill();
            var v = dim.gameObject.AddComponent<ShopView>();
            v.run = run; v.onChanged = onChanged; v.log = log; v.open = true;
            v.Build(dim.rectTransform);
            AudioKit.Sfx("ping", 0.6f);
            yield return v.PopIn(dim.rectTransform);
            while (v.open) yield return null;
            Destroy(dim.gameObject);
        }

        IEnumerator PopIn(RectTransform root)
        {
            var frame = root.Find("Frame");
            for (float t = 0; t < 1; t += Time.unscaledDeltaTime * 6f)
            {
                frame.localPosition = new Vector3(0, -400 * (1 - t) * (1 - t), 0) + frameHome;
                yield return null;
            }
            frame.localPosition = frameHome;
        }

        Vector3 frameHome;

        void Build(RectTransform root)
        {
            // Phone body
            var frame = UiKit.Panel(root, "Frame", UiKit.Hex("#15151C"), rounded: true);
            frame.rectTransform.Place(70, 110, 940, 1580);
            UiKit.Outline(frame, UiKit.Hex("#3A3A48"), 5);
            frameHome = frame.transform.localPosition;
            screen = UiKit.Panel(frame.transform, "Screen", UiKit.Hex("#FFF8EC"), rounded: true).rectTransform;
            screen.Place(20, 30, PW, PH, 940, 1580);

            // Header
            var head = UiKit.Panel(screen, "Header", Brand, rounded: true);
            head.rectTransform.Place(0, 0, PW, 170, PW, PH);
            var name = UiKit.Label(head.transform, "Name", "JhatPat", 70, UiKit.Ink, TextAnchor.UpperLeft, FontStyle.Bold);
            name.rectTransform.Place(36, 14, 500, 90, PW, 170);
            var tag = UiKit.Label(head.transform, "Tag", "delivered in 10 minutes*", 30, UiKit.Hex("#5D4A00"), TextAnchor.UpperLeft, FontStyle.Bold);
            tag.rectTransform.Place(40, 104, 560, 50, PW, 170);
            var pill = UiKit.Panel(head.transform, "Balance", UiKit.Hex("#1B5E20"), rounded: true);
            pill.rectTransform.Place(620, 40, 250, 90, PW, 170);
            balance = UiKit.Label(pill.transform, "T", "", 48, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            balance.rectTransform.Fill(4);

            // Tomorrow hint + what you hold
            var hint = UiKit.Label(screen, "Tomorrow", UiKit.Rich(TomorrowText()), 24, UiKit.Ink, TextAnchor.MiddleLeft);
            hint.rectTransform.Place(30, 176, PW - 60, 104, PW, PH);
            hint.lineSpacing = 0.9f;
            equipped = UiKit.Label(screen, "Equipped", "", 27, UiKit.Hex("#5D4037"), TextAnchor.MiddleLeft, FontStyle.Bold);
            equipped.rectTransform.Place(30, 282, PW - 60, 50, PW, PH);

            list = UiKit.Make("List", screen);
            list.Place(0, 336, PW, PH - 336, PW, PH);

            var close = UiKit.Button(root, "ShopClose", "Close JhatPat", UiKit.Orange, 46, () => { if (!busy) open = false; });
            close.GetComponent<RectTransform>().Place(240, 1736, 600, 150);

            Refresh();
        }

        string TomorrowText() => Tomorrow(run);

        /// <summary>Tomorrow's enemies with their types, and the weapon type that is super effective against them.</summary>
        public static string Tomorrow(Run run)
        {
            var ids = run.NextEnemyIds();
            string when = run.DayStarted ? "Tomorrow" : "Today";
            if (ids.Count == 0)
                return run.DayNumber >= run.TotalDays ? "Last day." : $"{when}: no fight. Stock up for later.";
            var parts = ids.Select(id => GameData.Combatants.TryGetValue(id, out var c) ? $"{c.name} ({(c.Ranged ? "Ranged" : "Close")})" : id);
            int ranged = ids.Count(id => GameData.Combatants.TryGetValue(id, out var c) && c.Ranged);
            string tip = ranged * 2 >= ids.Count ? "*Close* weapons are *super effective* against ranged enemies (+25%)." : "*Ranged* weapons shoot first and pick off the weakest; close enemies hit ranged heroes 25% harder.";
            return $"{when}: " + string.Join(", ", parts) + ". " + tip;
        }

        void Refresh()
        {
            var s = run.S;
            balance.text = "₹" + s.rupees;
            string w = s.weapon != null ? $"{s.weapon.data.name} ({s.weapon.fightsLeft} fights)" : "fists";
            string a = s.armour != null ? $"{s.armour.data.name} ({s.armour.fightsLeft})" : "none";
            equipped.text = $"Weapon: {w}   ·   Armour: {a}" + (s.fastags > 0 ? $"   ·   FASTag x{s.fastags}" : "");

            foreach (Transform c in list) Destroy(c.gameObject);
            float y = 0;
            BoxCard(y); y += 206;
            foreach (var it in s.shop) { ItemCard(it, y); y += 192; }
        }

        void BoxCard(float y)
        {
            var card = UiKit.Panel(list, "Box", UiKit.Hex("#4A148C"), rounded: true);
            card.rectTransform.Place(24, y, PW - 48, 196, PW, PH - 336);
            Icon(card.transform, "mystery_box", 14, 14, 168, PW - 48, 196);
            var title = UiKit.Label(card.transform, "Title", "Mystery Box", 42, Color.white, TextAnchor.UpperLeft, FontStyle.Bold);
            title.rectTransform.Place(200, 10, 420, 60, PW - 48, 196);
            var laathi = GameData.Items.TryGetValue("laathi", out var l) ? l : null;
            string odds = run.S.freeBoxUsed
                ? $"Anything in stock, or the <color=#FFC107><b>ULTRA RARE Laathi</b></color> ({(laathi != null ? Run.BoxOdds(laathi) : 0):0}%).\nMight be bubble wrap."
                : $"<color=#FFC107><b>Welcome offer: your first box is FREE.</b></color>\nCould be the ULTRA RARE Laathi ({(laathi != null ? Run.BoxOdds(laathi) : 0):0}%).";
            var desc = UiKit.Label(card.transform, "Desc", odds, 26, new Color(1, 1, 1, 0.85f), TextAnchor.UpperLeft);
            desc.rectTransform.Place(200, 70, 450, 120, PW - 48, 196);
            Buy(card.transform, run.BoxPrice, true, PW - 48, 196, () => StartCoroutine(OpenBox()));
        }

        void ItemCard(ItemData it, float y)
        {
            var card = UiKit.Panel(list, "Item_" + it.id, Color.white, rounded: true);
            card.rectTransform.Place(24, y, PW - 48, 180, PW, PH - 336);
            UiKit.Outline(card, UiKit.Hex("#E6D8BF"), 3);
            Icon(card.transform, "item_" + it.id, 14, 14, 152, PW - 48, 180);
            var title = UiKit.Label(card.transform, "Title", it.name, 38, UiKit.Ink, TextAnchor.UpperLeft, FontStyle.Bold);
            title.rectTransform.Place(184, 6, 300, 56, PW - 48, 180);
            title.horizontalOverflow = HorizontalWrapMode.Overflow;
            var chip = UiKit.Panel(card.transform, "Type", TypeColor(it), rounded: true);
            chip.rectTransform.Place(184 + Mathf.Min(330, title.preferredWidth + 14), 12, 150, 42, PW - 48, 180);
            var ct = UiKit.Label(chip.transform, "T", TypeTag(it), 22, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            ct.rectTransform.Fill();
            string lasts = it.slot switch { "weapon" or "armour" => $"\n<color=#E8590C><b>Lasts {it.fights} fights</b></color>", "pass" => "\n<color=#E8590C><b>One border</b></color>", "insurance" => $"\n<color=#E8590C><b>Expires in {it.fights} days</b></color>", _ => "" };
            var desc = UiKit.Label(card.transform, "Desc", it.desc + lasts, 24, UiKit.Hex("#4E4E4E"), TextAnchor.UpperLeft);
            desc.rectTransform.Place(184, 60, 470, 118, PW - 48, 180);
            desc.resizeTextForBestFit = true; desc.resizeTextMinSize = 16; desc.resizeTextMaxSize = 24;   // long text shrinks instead of being cut
            desc.verticalOverflow = VerticalWrapMode.Truncate;
            Buy(card.transform, it.price, false, PW - 48, 180, () => BuyItem(it, card.rectTransform));
        }

        void Icon(Transform parent, string sprite, float x, float y, float size, float pw, float ph)
        {
            var bg = UiKit.Panel(parent, "IconBg", new Color(0, 0, 0, 0.06f), rounded: true);
            bg.rectTransform.Place(x, y, size, size, pw, ph);
            var img = UiKit.Panel(bg.transform, "Icon", Color.white);
            img.sprite = Art.UI(sprite);
            img.preserveAspect = true;
            img.raycastTarget = false;
            img.rectTransform.Fill(8);
            if (img.sprite == null) { img.sprite = Art.Dot; img.color = new Color(1, 1, 1, 0.4f); }
        }

        void Buy(Transform card, int price, bool onDark, float pw, float ph, Action onBuy)
        {
            bool can = run.CanAfford(price);
            var b = UiKit.Button(card, "Buy", price == 0 ? "FREE" : "₹" + price, can ? UiKit.Green : new Color(0.62f, 0.62f, 0.62f), 40, () =>
            {
                if (busy) return;
                if (!run.CanAfford(price)) { AudioKit.Sfx("lose", 0.4f); Toast($"Need ₹{price - run.S.rupees} more."); return; }
                onBuy();
            });
            b.GetComponent<RectTransform>().Place(pw - 200, ph / 2 - 55, 176, 110, pw, ph);
        }

        void BuyItem(ItemData it, RectTransform card)
        {
            var notes = run.Buy(it);
            if (notes == null) return;
            AudioKit.Sfx("coin");
            Fx.Burst(this, screen, ScreenPos(card), Brand, 14, 600, 22, 0.6f);
            Toast(it.slot == "consumable" ? "Slurp. " + string.Join(", ", notes) : $"Delivered in 9 min {UnityEngine.Random.Range(40, 59)} sec!");
            log($"JhatPat: *{it.name}* delivered. " + string.Join(", ", notes), CardStyle.Good);
            onChanged();
            Refresh();
        }

        Vector2 ScreenPos(RectTransform rt) => screen.InverseTransformPoint(rt.TransformPoint(rt.rect.center));

        void Toast(string text)
        {
            var bg = UiKit.Panel(screen, "Toast", new Color(0.1f, 0.1f, 0.12f, 0.92f), rounded: true);
            bg.rectTransform.Place(150, PH - 170, PW - 300, 100, PW, PH);
            var t = UiKit.Label(bg.transform, "T", text, 34, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            t.rectTransform.Fill(10);
            StartCoroutine(FadeToast(bg));
        }

        IEnumerator FadeToast(Image bg)
        {
            var g = bg.gameObject.AddComponent<CanvasGroup>();
            for (float t = 0; t < 1.6f; t += Time.unscaledDeltaTime) { g.alpha = t < 0.15f ? t / 0.15f : t > 1.2f ? (1.6f - t) / 0.4f : 1; yield return null; }
            Destroy(bg.gameObject);
        }

        // ------------------------------------------------------------------ Mystery Box

        IEnumerator OpenBox()
        {
            var it = run.OpenBox();
            if (it == null) yield break;
            busy = true;
            onChanged();
            Refresh();

            var dim = UiKit.Panel(transform, "BoxOpen", new Color(0, 0, 0, 0.85f));
            dim.rectTransform.Fill();
            var root = dim.rectTransform;

            // 1. The box shakes harder and harder
            var box = UiKit.Panel(root, "Box", Color.white);
            box.sprite = Art.UI("mystery_box") ?? Art.Dot;
            box.preserveAspect = true;
            box.rectTransform.Place(290, 560, 500, 500);
            AudioKit.Sfx("whoosh");
            for (float t = 0; t < 1.4f; t += Time.unscaledDeltaTime)
            {
                float k = t / 1.4f, amp = 4 + 26 * k * k;
                box.rectTransform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * 40f) * amp);
                box.rectTransform.localScale = Vector3.one * (1f + 0.15f * k);
                if (Mathf.FloorToInt(t * 8) != Mathf.FloorToInt((t - Time.unscaledDeltaTime) * 8)) AudioKit.SfxVaried("card", 0.4f + 0.4f * k, 0.02f);
                yield return null;
            }

            // 2. Pop!
            Color rc = RarityColor(it.rarity);
            bool ultra = it.rarity == "ultra", junk = it.slot == "junk";
            Destroy(box.gameObject);
            var flash = UiKit.Panel(root, "Flash", rc);
            flash.rectTransform.Fill();
            AudioKit.Sfx(ultra ? "levelup" : junk ? "lose" : "win");
            AudioKit.Sfx("boom", 0.6f);
            Fx.Burst(this, root, new Vector2(0, 150), rc, ultra ? 60 : 30, ultra ? 1400 : 900, ultra ? 34 : 24, 1.1f, 600);
            if (ultra) for (int i = 0; i < Fx.Confetti.Length; i++) Fx.Burst(this, root, new Vector2(UnityEngine.Random.Range(-400f, 400f), 700), Fx.Confetti[i], 12, 700, 22, 1.6f, 900, square: true);

            var rays = UiKit.Panel(root, "Glow", new Color(rc.r, rc.g, rc.b, 0.35f));
            rays.sprite = Art.UI("burst") ?? Art.Dot;
            rays.rectTransform.Place(190, 460, 700, 700);
            var icon = UiKit.Panel(root, "Item", Color.white);
            icon.sprite = Art.UI("item_" + it.id) ?? Art.Dot;
            icon.preserveAspect = true;
            icon.rectTransform.Place(340, 610, 400, 400);

            var rarity = UiKit.Label(root, "Rarity", RarityName(it.rarity) + "!", ultra ? 96 : 64, rc, TextAnchor.MiddleCenter, FontStyle.Bold);
            rarity.rectTransform.Place(40, 300, 1000, 140);
            rarity.gameObject.AddComponent<TextFx>().wave = ultra ? 8f : 4f;
            UiKit.Outline(rarity, UiKit.Ink, 5);
            var title = UiKit.Label(root, "Name", it.name, 72, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            title.rectTransform.Place(40, 1060, 1000, 100);
            UiKit.Outline(title, UiKit.Ink, 4);
            string lasts = it.IsGear ? $"  <color=#FFB74D>Lasts {it.fights} fights.</color>" : "";
            var desc = UiKit.Label(root, "Desc", it.desc + lasts, 36, new Color(1, 1, 1, 0.85f), TextAnchor.UpperCenter);
            desc.rectTransform.Place(90, 1170, 900, 160);

            for (float t = 0; t < 1; t += Time.unscaledDeltaTime * 3f)
            {
                flash.color = new Color(rc.r, rc.g, rc.b, 0.9f * (1 - t));
                float s = t < 0.6f ? Mathf.Lerp(0.2f, 1.15f, t / 0.6f) : Mathf.Lerp(1.15f, 1f, (t - 0.6f) / 0.4f);
                icon.rectTransform.localScale = Vector3.one * s;
                yield return null;
            }
            icon.rectTransform.localScale = Vector3.one; // a slow frame can end the loop early (seen in the browser pane)
            Destroy(flash.gameObject);
            if (ultra) log("JhatPat Mystery Box: *ULTRA RARE Laathi!* The whole society WhatsApp group is talking about it.", CardStyle.Good);

            // 3. Keep it or sell it back
            int choice = -1;
            string keep = it.slot switch
            {
                "weapon" => run.S.weapon != null ? $"Equip (drop {run.S.weapon.data.name})" : "Equip",
                "armour" => run.S.armour != null ? $"Wear (drop {run.S.armour.data.name})" : "Wear",
                "consumable" => "Drink it",
                "pass" => "Keep it",
                _ => "Keep it",
            };
            var keepBtn = UiKit.Button(root, "BoxKeep", keep, UiKit.Green, 38, () => choice = 0);
            int value = run.SellValue(it);
            if (junk) keepBtn.GetComponent<RectTransform>().Place(240, 1400, 600, 150);
            else
            {
                keepBtn.GetComponent<RectTransform>().Place(40, 1400, 560, 150);
                var sell = UiKit.Button(root, "BoxSell", $"Return for ₹{value}", UiKit.Purple, 38, () => choice = 1);
                sell.GetComponent<RectTransform>().Place(620, 1400, 420, 150);
            }
            float spin = 0;
            while (choice < 0)
            {
                spin += Time.unscaledDeltaTime * 30f;
                rays.rectTransform.localRotation = Quaternion.Euler(0, 0, spin);
                icon.rectTransform.anchoredPosition += new Vector2(0, Mathf.Sin(Time.unscaledTime * 3f) * 0.4f);
                yield return null;
            }

            if (choice == 0)
            {
                if (junk) { AudioKit.SfxVaried("slap", 0.6f, 0.2f); log("JhatPat Mystery Box: *Bubble wrap*. You pop every single bubble. Worth it.", CardStyle.Note); }
                else
                {
                    var notes = run.Take(it);
                    AudioKit.Sfx("coin");
                    if (!ultra) log($"JhatPat Mystery Box: *{it.name}* ({RarityName(it.rarity).ToLower()}). " + string.Join(", ", notes), CardStyle.Good);
                    else log(string.Join(", ", notes), CardStyle.Note);
                }
            }
            else
            {
                run.Sell(it);
                AudioKit.Sfx("coin");
                log($"JhatPat Mystery Box: *{it.name}*. Returned for ₹{value}. The refund arrives in 5-7 business days. Instantly.", CardStyle.Note);
            }
            onChanged();
            Destroy(dim.gameObject);
            busy = false;
            Refresh();
        }
    }
}
