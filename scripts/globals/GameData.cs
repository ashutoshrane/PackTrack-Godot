using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Central data store for PackTrack — parachute packing management for skydiving drop zones.
/// Manages rigs, pack logs, skydivers, and billing.
/// Autoload singleton: add to Project → AutoLoad as "GameData".
/// </summary>
public partial class GameData : Node
{
	// ── Signals ──────────────────────────────────────────────────────────────────
	[Signal] public delegate void PackLogAddedEventHandler(string rigId);
	[Signal] public delegate void RigUpdatedEventHandler(string rigId);

	// ── Constants ────────────────────────────────────────────────────────────────
	public const int REPACK_CYCLE_DAYS = 180;
	public const int WARNING_THRESHOLD_DAYS = 30;

	// ── Properties ───────────────────────────────────────────────────────────────

	/// <summary>Current logged-in user. Keys: "name", "role", "id". Starts empty for onboarding.</summary>
	public Godot.Collections.Dictionary CurrentUser { get; set; } = new Godot.Collections.Dictionary();

	/// <summary>All rigs registered at the drop zone.</summary>
	public List<Godot.Collections.Dictionary> Rigs { get; set; } = new List<Godot.Collections.Dictionary>();

	/// <summary>Every pack job recorded.</summary>
	public List<Godot.Collections.Dictionary> PackLogs { get; set; } = new List<Godot.Collections.Dictionary>();

	/// <summary>Registered skydivers / customers.</summary>
	public List<Godot.Collections.Dictionary> Skydivers { get; set; } = new List<Godot.Collections.Dictionary>();

	// ── Lifecycle ────────────────────────────────────────────────────────────────

	public override void _Ready()
	{
		PopulateSampleData();
	}

	// ── Public API ───────────────────────────────────────────────────────────────

	/// <summary>
	/// Record a new pack job. Creates a timestamped log entry, increments the
	/// rig's pack count, updates its last-packed date, and fires signals.
	/// </summary>
	public void AddPackLog(string rigId, string packer, string skydiver, float amount)
	{
		string logId = Guid.NewGuid().ToString().Substring(0, 8);
		string now = DateTime.Now.ToString("yyyy-MM-dd HH:mm");

		var log = new Godot.Collections.Dictionary
		{
			{ "id", logId },
			{ "rigId", rigId },
			{ "packer", packer },
			{ "skydiver", skydiver },
			{ "amount", amount },
			{ "date", now },
			{ "settled", false }
		};

		PackLogs.Add(log);

		// Update the rig record
		var rig = GetRigById(rigId);
		if (rig != null && rig.Count > 0)
		{
			rig["lastPacked"] = DateTime.Now.ToString("yyyy-MM-dd");
			int currentCount = rig.ContainsKey("packCount") ? Convert.ToInt32(rig["packCount"]) : 0;
			rig["packCount"] = currentCount + 1;
			rig["status"] = "ok";

			EmitSignal(SignalName.RigUpdated, rigId);
		}

		EmitSignal(SignalName.PackLogAdded, rigId);
	}

	/// <summary>Find a rig by its ID string. Returns an empty Dictionary if not found.</summary>
	public Godot.Collections.Dictionary GetRigById(string id)
	{
		foreach (var rig in Rigs)
		{
			if (rig.ContainsKey("id") && rig["id"].ToString() == id)
				return rig;
		}
		return new Godot.Collections.Dictionary();
	}

	/// <summary>All pack logs recorded today.</summary>
	public List<Godot.Collections.Dictionary> GetPacksForToday()
	{
		string today = DateTime.Now.ToString("yyyy-MM-dd");
		return PackLogs
			.Where(log => log.ContainsKey("date") && log["date"].ToString().StartsWith(today))
			.ToList();
	}

	/// <summary>All pack logs attributed to a specific packer.</summary>
	public List<Godot.Collections.Dictionary> GetPacksByPacker(string name)
	{
		return PackLogs
			.Where(log => log.ContainsKey("packer") && log["packer"].ToString() == name)
			.ToList();
	}

	/// <summary>All pack-log charges that have not yet been settled / paid out.</summary>
	public List<Godot.Collections.Dictionary> GetUnsettledCharges()
	{
		return PackLogs
			.Where(log => log.ContainsKey("settled") && !(bool)log["settled"])
			.ToList();
	}

	/// <summary>Sum of amounts for today's pack logs.</summary>
	public float GetTotalEarningsToday()
	{
		return (float)GetPacksForToday()
			.Where(log => log.ContainsKey("amount"))
			.Sum(log => Convert.ToSingle(log["amount"]));
	}

	/// <summary>
	/// Evaluate a rig's repack status.
	/// Returns "ok", "warning", or "overdue" based on days since last pack
	/// relative to the FAA 180-day repack cycle.
	/// </summary>
	public string CheckRepackStatus(Godot.Collections.Dictionary rig)
	{
		if (rig == null || !rig.ContainsKey("lastPacked"))
			return "overdue";

		string lastPackedStr = rig["lastPacked"].ToString();
		if (!DateTime.TryParse(lastPackedStr, out DateTime lastPacked))
			return "overdue";

		int daysSincePack = (DateTime.Now - lastPacked).Days;

		if (daysSincePack >= REPACK_CYCLE_DAYS)
			return "overdue";
		if (daysSincePack >= REPACK_CYCLE_DAYS - WARNING_THRESHOLD_DAYS)
			return "warning";
		return "ok";
	}

	/// <summary>Mark a charge as settled / paid.</summary>
	public void SettleCharge(string logId)
	{
		foreach (var log in PackLogs)
		{
			if (log.ContainsKey("id") && log["id"].ToString() == logId)
			{
				log["settled"] = true;
				return;
			}
		}
	}

	// ── Sample Data ──────────────────────────────────────────────────────────────

	/// <summary>
	/// Populate realistic sample data so the app is usable immediately.
	/// Covers 4 skydivers, 8 rigs (main + reserve combos plus a tandem),
	/// and 15 pack-log entries with mixed settlement states.
	/// </summary>
	private void PopulateSampleData()
	{
		// ── Skydivers ────────────────────────────────────────────────────
		Skydivers = new List<Godot.Collections.Dictionary>
		{
			new Godot.Collections.Dictionary
			{
				{ "id", "sky-001" }, { "name", "Sarah Chen" },
				{ "license", "D-34892" }, { "jumps", 1420 }
			},
			new Godot.Collections.Dictionary
			{
				{ "id", "sky-002" }, { "name", "Marcus Rodriguez" },
				{ "license", "C-18734" }, { "jumps", 620 }
			},
			new Godot.Collections.Dictionary
			{
				{ "id", "sky-003" }, { "name", "Emily Foster" },
				{ "license", "B-45021" }, { "jumps", 285 }
			},
			new Godot.Collections.Dictionary
			{
				{ "id", "sky-004" }, { "name", "Jake Morrison" },
				{ "license", "D-51200" }, { "jumps", 3150 }
			}
		};

		// Helper dates
		string today = DateTime.Now.ToString("yyyy-MM-dd");
		string threeDaysAgo = DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd");
		string oneWeekAgo = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd");
		string twoWeeksAgo = DateTime.Now.AddDays(-14).ToString("yyyy-MM-dd");
		string oneMonthAgo = DateTime.Now.AddDays(-30).ToString("yyyy-MM-dd");
		string warningDate = DateTime.Now.AddDays(-(REPACK_CYCLE_DAYS - WARNING_THRESHOLD_DAYS + 5)).ToString("yyyy-MM-dd");
		string overdueDate = DateTime.Now.AddDays(-(REPACK_CYCLE_DAYS + 10)).ToString("yyyy-MM-dd");

		// ── Rigs ─────────────────────────────────────────────────────────
		Rigs = new List<Godot.Collections.Dictionary>
		{
			new Godot.Collections.Dictionary
			{
				{ "id", "N4521-Main" }, { "owner", "Sarah Chen" }, { "ownerId", "sky-001" },
				{ "canopy", "Sabre 170" }, { "container", "Javelin J4" },
				{ "type", "main" }, { "lastPacked", threeDaysAgo },
				{ "packCount", 47 }, { "status", "ok" }
			},
			new Godot.Collections.Dictionary
			{
				{ "id", "N4521-Rsv" }, { "owner", "Sarah Chen" }, { "ownerId", "sky-001" },
				{ "canopy", "PD Reserve 176" }, { "container", "Javelin J4" },
				{ "type", "reserve" }, { "lastPacked", oneMonthAgo },
				{ "packCount", 8 }, { "status", "ok" }
			},
			new Godot.Collections.Dictionary
			{
				{ "id", "N7833-Main" }, { "owner", "Marcus Rodriguez" }, { "ownerId", "sky-002" },
				{ "canopy", "Safire 149" }, { "container", "Mirage G4.1" },
				{ "type", "main" }, { "lastPacked", oneWeekAgo },
				{ "packCount", 23 }, { "status", "ok" }
			},
			new Godot.Collections.Dictionary
			{
				{ "id", "N6290-Main" }, { "owner", "Emily Foster" }, { "ownerId", "sky-003" },
				{ "canopy", "Sabre2 150" }, { "container", "Vector V348" },
				{ "type", "main" }, { "lastPacked", twoWeeksAgo },
				{ "packCount", 12 }, { "status", "ok" }
			},
			new Godot.Collections.Dictionary
			{
				{ "id", "N6290-Rsv" }, { "owner", "Emily Foster" }, { "ownerId", "sky-003" },
				{ "canopy", "PD Reserve 160" }, { "container", "Vector V348" },
				{ "type", "reserve" }, { "lastPacked", warningDate },
				{ "packCount", 4 }, { "status", "warning" }
			},
			new Godot.Collections.Dictionary
			{
				{ "id", "N5117-Main" }, { "owner", "Jake Morrison" }, { "ownerId", "sky-004" },
				{ "canopy", "Katana 120" }, { "container", "Infinity I-44" },
				{ "type", "main" }, { "lastPacked", today },
				{ "packCount", 89 }, { "status", "ok" }
			},
			new Godot.Collections.Dictionary
			{
				{ "id", "N5117-Rsv" }, { "owner", "Jake Morrison" }, { "ownerId", "sky-004" },
				{ "canopy", "PD Reserve 126" }, { "container", "Infinity I-44" },
				{ "type", "reserve" }, { "lastPacked", overdueDate },
				{ "packCount", 6 }, { "status", "overdue" }
			},
			new Godot.Collections.Dictionary
			{
				{ "id", "N8845-Tandem" }, { "owner", "DZ Fleet" }, { "ownerId", "dz-001" },
				{ "canopy", "Sigma 370 Tandem" }, { "container", "Strong Dual Hawk" },
				{ "type", "tandem" }, { "lastPacked", threeDaysAgo },
				{ "packCount", 214 }, { "status", "ok" }
			}
		};

		// ── Pack Logs (15 entries) ─────────────────────────────────────────
		string todayTime1 = DateTime.Now.ToString("yyyy-MM-dd") + " 08:15";
		string todayTime2 = DateTime.Now.ToString("yyyy-MM-dd") + " 09:42";
		string todayTime3 = DateTime.Now.ToString("yyyy-MM-dd") + " 11:05";

		PackLogs = new List<Godot.Collections.Dictionary>
		{
			new Godot.Collections.Dictionary
			{
				{ "id", "pl-001" }, { "rigId", "N5117-Main" }, { "packer", "Tony Alvarez" },
				{ "skydiver", "Jake Morrison" }, { "amount", 8.0f },
				{ "date", todayTime1 }, { "settled", false }
			},
			new Godot.Collections.Dictionary
			{
				{ "id", "pl-002" }, { "rigId", "N4521-Main" }, { "packer", "Lisa Park" },
				{ "skydiver", "Sarah Chen" }, { "amount", 8.0f },
				{ "date", todayTime2 }, { "settled", false }
			},
			new Godot.Collections.Dictionary
			{
				{ "id", "pl-003" }, { "rigId", "N8845-Tandem" }, { "packer", "Tony Alvarez" },
				{ "skydiver", "DZ Fleet" }, { "amount", 12.0f },
				{ "date", todayTime3 }, { "settled", false }
			},
			new Godot.Collections.Dictionary
			{
				{ "id", "pl-004" }, { "rigId", "N7833-Main" }, { "packer", "Lisa Park" },
				{ "skydiver", "Marcus Rodriguez" }, { "amount", 8.0f },
				{ "date", DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd") + " 10:30" },
				{ "settled", true }
			},
			new Godot.Collections.Dictionary
			{
				{ "id", "pl-005" }, { "rigId", "N6290-Main" }, { "packer", "Tony Alvarez" },
				{ "skydiver", "Emily Foster" }, { "amount", 8.0f },
				{ "date", DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd") + " 14:20" },
				{ "settled", true }
			},
			new Godot.Collections.Dictionary
			{
				{ "id", "pl-006" }, { "rigId", "N4521-Main" }, { "packer", "Tony Alvarez" },
				{ "skydiver", "Sarah Chen" }, { "amount", 8.0f },
				{ "date", DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd") + " 09:00" },
				{ "settled", true }
			},
			new Godot.Collections.Dictionary
			{
				{ "id", "pl-007" }, { "rigId", "N5117-Main" }, { "packer", "Lisa Park" },
				{ "skydiver", "Jake Morrison" }, { "amount", 8.0f },
				{ "date", DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd") + " 11:45" },
				{ "settled", false }
			},
			new Godot.Collections.Dictionary
			{
				{ "id", "pl-008" }, { "rigId", "N8845-Tandem" }, { "packer", "Tony Alvarez" },
				{ "skydiver", "DZ Fleet" }, { "amount", 12.0f },
				{ "date", DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd") + " 08:30" },
				{ "settled", true }
			},
			new Godot.Collections.Dictionary
			{
				{ "id", "pl-009" }, { "rigId", "N7833-Main" }, { "packer", "Tony Alvarez" },
				{ "skydiver", "Marcus Rodriguez" }, { "amount", 8.0f },
				{ "date", DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd") + " 13:15" },
				{ "settled", false }
			},
			new Godot.Collections.Dictionary
			{
				{ "id", "pl-010" }, { "rigId", "N4521-Rsv" }, { "packer", "Lisa Park" },
				{ "skydiver", "Sarah Chen" }, { "amount", 25.0f },
				{ "date", DateTime.Now.AddDays(-5).ToString("yyyy-MM-dd") + " 10:00" },
				{ "settled", true }
			},
			new Godot.Collections.Dictionary
			{
				{ "id", "pl-011" }, { "rigId", "N6290-Main" }, { "packer", "Lisa Park" },
				{ "skydiver", "Emily Foster" }, { "amount", 8.0f },
				{ "date", DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd") + " 15:30" },
				{ "settled", true }
			},
			new Godot.Collections.Dictionary
			{
				{ "id", "pl-012" }, { "rigId", "N5117-Main" }, { "packer", "Tony Alvarez" },
				{ "skydiver", "Jake Morrison" }, { "amount", 8.0f },
				{ "date", DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd") + " 09:20" },
				{ "settled", true }
			},
			new Godot.Collections.Dictionary
			{
				{ "id", "pl-013" }, { "rigId", "N8845-Tandem" }, { "packer", "Lisa Park" },
				{ "skydiver", "DZ Fleet" }, { "amount", 12.0f },
				{ "date", DateTime.Now.AddDays(-10).ToString("yyyy-MM-dd") + " 12:00" },
				{ "settled", false }
			},
			new Godot.Collections.Dictionary
			{
				{ "id", "pl-014" }, { "rigId", "N4521-Main" }, { "packer", "Tony Alvarez" },
				{ "skydiver", "Sarah Chen" }, { "amount", 8.0f },
				{ "date", DateTime.Now.AddDays(-12).ToString("yyyy-MM-dd") + " 16:45" },
				{ "settled", true }
			},
			new Godot.Collections.Dictionary
			{
				{ "id", "pl-015" }, { "rigId", "N6290-Rsv" }, { "packer", "Tony Alvarez" },
				{ "skydiver", "Emily Foster" }, { "amount", 25.0f },
				{ "date", DateTime.Now.AddDays(-155).ToString("yyyy-MM-dd") + " 10:00" },
				{ "settled", true }
			}
		};
	}
}
