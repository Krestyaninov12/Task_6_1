using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Mechanical;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Prism.Commands;
using RevitAPITrainingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Task_6_1
{
    public class MainViewViewModel
    {
        private ExternalCommandData _commandData;

        public List<DuctType> DuctTypes { get; } = new List<DuctType>();
        public List<Level> Levels { get; } = new List<Level>();
        public DelegateCommand SaveCommand { get; }
        public double DuctOffset { get; set; }
        public List<XYZ> Points { get; } = new List<XYZ>();
        public MEPSystemType DuctSystemType { get; }
        public DuctType SelectedDuctType { get; set; }
        public Level SelectedLevel { get; set; }

        public MainViewViewModel(ExternalCommandData commandData)
        {
            _commandData = commandData;
            DuctTypes = DuctUtils.GetDuctTypes(commandData);
            Levels = LevelsUtils.GetLevels(commandData);
            SaveCommand = new DelegateCommand(onSaveCommand);
            DuctOffset = 0;
            Points = SelectionUtils.GetPoints(_commandData, "Выберите точки", ObjectSnapTypes.Endpoints);
            DuctSystemType = DuctUtils.GetDuctSystemTypes(_commandData);
        }
        private void onSaveCommand()
        {
            UIApplication uiapp = _commandData.Application;
            UIDocument uidoc = uiapp.ActiveUIDocument;
            Document doc = uidoc.Document;

            if (Points.Count < 2 ||
                SelectedDuctType == null ||
                SelectedLevel == null)
                return;

            using (var ts = new Transaction(doc, "Create duct"))
            {
                ts.Start();
                for (int i = 0; i < Points.Count; i++)
                {
                    if (i == 0)
                        continue;

                    var prevPoint = Points[i - 1];
                    var currentPoint = Points[i];

                    Duct duct = Duct.Create(doc, DuctSystemType.Id, SelectedDuctType.Id, SelectedLevel.Id, prevPoint, currentPoint);
                    Parameter ductOffset = duct.get_Parameter(BuiltInParameter.RBS_DUCT_BOTTOM_ELEVATION);
                    ductOffset.Set(DuctOffset / 304.80000000000217); // Почему-то конвертация через ConvertFromInternalUnits работает некорректно 
                    //ductOffset.Set(UnitUtils.ConvertFromInternalUnits(DuctOffset, UnitTypeId.Millimeters));
                }
                ts.Commit();

            }
            RaiseCloseRequest();
        }

        public event EventHandler CloseRequest;
        private void RaiseCloseRequest()
        {
            CloseRequest?.Invoke(this, EventArgs.Empty);
        }

    }
}
