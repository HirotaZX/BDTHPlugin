using System;
using System.Numerics;

using Dalamud.Interface.Components;
using Dalamud.Interface.Windowing;

using ImGuiNET;
using ImGuizmoNET;

using BDTHPlugin.Interface.Components;

namespace BDTHPlugin.Interface.Windows
{
  public class MainWindow : Window
  {
    private static PluginMemory Memory => Plugin.GetMemory();
    private static Configuration Configuration => Plugin.GetConfiguration();

    private static readonly Vector4 RED_COLOR = new(1, 0, 0, 1);

    private readonly Gizmo Gizmo;
    private readonly ItemControls ItemControls = new();

    public bool Reset;

    public MainWindow(Gizmo gizmo) : base(
      "Burning Down the House##BDTH",
      ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.NoResize |
      ImGuiWindowFlags.AlwaysAutoResize
    )
    {
      Gizmo = gizmo;
    }

    public override void PreDraw()
    {
      if (Reset)
      {
        Reset = false;
        ImGui.SetNextWindowPos(new Vector2(69, 69), ImGuiCond.Always);
      }
    }

    public unsafe override void Draw()
    {
      ImGui.BeginGroup();

      var placeAnywhere = Configuration.PlaceAnywhere;
      if (ImGui.Checkbox("任意放置", ref placeAnywhere))
      {
        // Set the place anywhere based on the checkbox state.
        Memory.SetPlaceAnywhere(placeAnywhere);
        Configuration.PlaceAnywhere = placeAnywhere;
        Configuration.Save();
      }
      DrawTooltip("解除游戏引擎的家具摆放限制");

      ImGui.SameLine();

      // Checkbox is clicked, set the configuration and save.
      var useGizmo = Configuration.UseGizmo;
      if (ImGui.Checkbox("轴向移动工具", ref useGizmo))
      {
        Configuration.UseGizmo = useGizmo;
        Configuration.Save();
      }
      DrawTooltip("在选定家具上显示一个移动控制器，可以沿着轴方向进行拖动");

      ImGui.SameLine();

      // Checkbox is clicked, set the configuration and save.
      var doSnap = Configuration.DoSnap;
      if (ImGui.Checkbox("网格对齐", ref doSnap))
      {
        Configuration.DoSnap = doSnap;
        Configuration.Save();
      }
      DrawTooltip("根据下方的网格距离开启轴向移动的网格对齐");

      ImGui.SameLine();
      if (ImGuiComponents.IconButton(1, Gizmo.Mode == MODE.LOCAL ? Dalamud.Interface.FontAwesomeIcon.ArrowsAlt : Dalamud.Interface.FontAwesomeIcon.Globe))
        Gizmo.Mode = Gizmo.Mode == MODE.LOCAL ? MODE.WORLD : MODE.LOCAL;

      DrawTooltip(new[]
      {
        $"模式: {(Gizmo.Mode == MODE.LOCAL ? "本地" : "世界")}",
        "切换本地坐标轴和世界坐标轴"
      });

      ImGui.Separator();

      if (Memory.HousingStructure->Mode == HousingLayoutMode.None)
        DrawError("请先进入装修模式");
      else if (PluginMemory.GamepadMode)
        DrawError("暂不支持手柄模式");
      else if (Memory.HousingStructure->ActiveItem == null || Memory.HousingStructure->Mode != HousingLayoutMode.Rotate)
      {
        DrawError("请先在旋转模式下选中一个家具");
        ImGuiComponents.HelpMarker("Are you doing everything right? Try using the /bdth debug command and report this issue in Discord!");
      }
      else
        ItemControls.Draw();

      ImGui.Separator();

      // Drag amount for the inputs.
      var drag = Configuration.Drag;
      if (ImGui.InputFloat("网格大小", ref drag, 0.05f))
      {
        drag = Math.Min(Math.Max(0.001f, drag), 10f);
        Configuration.Drag = drag;
        Configuration.Save();
      }
      DrawTooltip("设置拖拽和对齐的网格大小");

      var dummyHousingGoods = PluginMemory.HousingGoods != null && PluginMemory.HousingGoods->IsVisible;
      var dummyInventory = Memory.InventoryVisible;

      if (ImGui.Checkbox("显示家具设置", ref dummyHousingGoods))
        if (PluginMemory.HousingGoods != null)
          PluginMemory.HousingGoods->IsVisible = dummyHousingGoods;
      ImGui.SameLine();

      if (ImGui.Checkbox("显示物品栏", ref dummyInventory))
        Memory.InventoryVisible = dummyInventory;

      if (ImGui.Button("打开家具列表"))
        Plugin.CommandManager.ProcessCommand("/bdth list");
      DrawTooltip(new[]
      {
        "打开一个可以距离排序并选择家具的列表",
        "注意: 暂不支持庭具!"
      });

      var autoVisible = Configuration.AutoVisible;
      if (ImGui.Checkbox("自动打开", ref autoVisible))
      {
        Configuration.AutoVisible = autoVisible;
        Configuration.Save();
      }
    }

    private static void DrawTooltip(string[] text)
    {
      if (ImGui.IsItemHovered())
      {
        ImGui.BeginTooltip();
        foreach (var t in text)
          ImGui.Text(t);
        ImGui.EndTooltip();
      }
    }

    private static void DrawTooltip(string text)
    {
      DrawTooltip(new[] { text });
    }

    private void DrawError(string text)
    {
      ImGui.PushStyleColor(ImGuiCol.Text, RED_COLOR);
      ImGui.Text(text);
      ImGui.PopStyleColor();
    }
  }
}
