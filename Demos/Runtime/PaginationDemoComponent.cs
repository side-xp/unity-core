using System.Collections.Generic;
using UnityEngine;
using SideXP.Core;

// Illustrates the usage of Pagination struct.
[AddComponentMenu(Constants.AddComponentMenuDemosCore + "/Pagination Demo")]
public class PaginationDemoComponent : MonoBehaviour
{
    private const string DemoStr = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    public Pagination Pagination = new Pagination(0, 5);
    private List<string> _list = new List<string>();

    private void Awake()
    {
        // Split all characters of the demo string into a list
        for (int i = 0; i < DemoStr.Length - 1; i++)
            _list.Add(DemoStr.Substring(i, 1));
    }

    private void OnGUI()
    {
        using (new GUILayout.VerticalScope(GUI.skin.box))
        {
            GUILayout.Box("Pagination Settings", GUI.skin.box.Bold());

            // Draw page navigation
            using (new GUILayout.HorizontalScope())
            {
                if (GUILayout.Button("<"))
                    Pagination.Page--;

                GUILayout.Label((Pagination.Page + 1).ToString(), GUI.skin.label.AlignCenter());

                if (GUILayout.Button(">"))
                    Pagination.Page++;
            }

            // Draw "elements per page" field
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("Elements per page");
                Pagination.NbElementsPerPage = MoreGUI.IntField(Pagination.NbElementsPerPage);
            }

            // Display paginated elements
            GUILayout.Box("Paginated List");
            foreach (string str in Pagination.Paginate(_list))
            {
                GUILayout.Label(str);
            }
        }
    }
}
