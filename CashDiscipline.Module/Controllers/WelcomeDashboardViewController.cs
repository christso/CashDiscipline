using DevExpress.ExpressApp;
using DevExpress.ExpressApp.Editors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CashDiscipline.Module.Controllers
{
    public class WelcomeDashboardViewController : ViewController<DashboardView>
    {
        public WelcomeDashboardViewController()
        {
            TargetViewId = "Welcome_Dashboard";
        }

        protected override void OnActivated()
        {
            base.OnActivated();
        }

        protected override void OnViewControlsCreated()
        {
            base.OnViewControlsCreated();

            var item = (StaticText)View.FindItem("Welcome");
            /*
            item.Text =
@"<p><strong>Welcome to the Cash Discipline WinForms application. This is a data-capture and data-transformation tool used for daily cash flow analysis.</strong></p>
<p><strong>Important Links:</strong></p>
<table>
<tbody>
<tr>
<td>Working Capital Reports</td>
<td>\\vhacorp07.vha.internal\vodashare$\Finance\Cash Reports\Working Capital</td>
</tr>
<tr>
<td>Customer Reports</td>
<td>\\vhacorp07.vha.internal\vodashare$\Finance\Cash Reports\Customer Investment</td>
</tr>
<tr>
<td>Accounting Reports</td>
<td>\\vhacorp07.vha.internal\vodashare$\Finance\Cash Reports\Accounting</td>
</tr>
</tbody>
</table>
<p><strong>Server Details:</strong></p>
<table>
<tbody>
<tr>
<td>Hostname:</td>
<td>{svr_hostname}</td>
</tr>
<tr>
<td>IP Address:</td>
<td>{svr_ipaddress}</td>
</tr>
<tr>
<td>Data Source:</td>
<td>{svr_datasource}</td>
</tr>
</tbody>
</table>
<p><strong>User Details:&nbsp;</strong></p>
<table>
<tbody>
<tr>
<td>Username:</td>
<td>{username}</td>
</tr>
<tr>
<td>Hostname:</td>
<td>{user_hostname}</td>
</tr>
<tr>
<td>IP Address:</td>
<td>{user_ipaddress}</td>
</tr>
<tr>
<td>Application Path:</td>
<td>{apppath}</td>
</tr>
</tbody>
</table>";
*/
        }
    }
}
