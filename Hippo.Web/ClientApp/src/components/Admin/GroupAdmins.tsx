import { useEffect, useState, useMemo } from "react";
import { useParams } from "react-router-dom";
import { GroupAdminModel, IRouteParams } from "../../types";
import { authenticatedFetch } from "../../util/api";
import { ReactTable } from "../../Shared/ReactTable";
import { Column } from "react-table";
import { GroupNameWithTooltip } from "../Group/GroupNameWithTooltip";
import { getGroupModelString } from "../../util/StringHelpers";

export const GroupAdmins = () => {
  // get all accounts that need approval and list them
  // allow user to approve or reject each account
  const [groupAdmins, setGroupAdmins] = useState<GroupAdminModel[]>();
  const { cluster } = useParams<IRouteParams>();

  useEffect(() => {
    const fetchGroupAdmins = async () => {
      const response = await authenticatedFetch(
        `/api/${cluster}/admin/GroupAdmins`
      );

      if (response.ok) {
        setGroupAdmins(await response.json());
      } else {
        alert("Error fetching group admins");
      }
    };

    fetchGroupAdmins();
  }, [cluster]);

  const columns: Column<GroupAdminModel>[] = useMemo(
    () => [
      {
        Header: "Group",
        accessor: (row) => getGroupModelString(row.group),
        Cell: (props) => (
          <GroupNameWithTooltip
            group={props.row.original.group}
            showDisplayName={false}
          />
        ),
        sortable: true,
      },
      {
        Header: "User",
        accessor: (ga) => ga.account.name,
        sortable: true,
      },
      {
        Header: "Email",
        accessor: (ga) => ga.account.email,
        sortable: true,
      },
    ],
    []
  );

  const groupAdminsData = useMemo(() => groupAdmins ?? [], [groupAdmins]);

  if (groupAdmins === undefined) {
    return (
      <div className="row justify-content-center">
        <div className="col-md-8">Loading...</div>
      </div>
    );
  } else {
    return (
      <div className="row justify-content-center">
        <div className="col-md-8">
          <p>There are {groupAdmins.length} group admins</p>
          <ReactTable
            columns={columns}
            data={groupAdminsData}
            initialState={{
              sortBy: [{ id: "lastName" }, { id: "firstName" }],
            }}
          />
        </div>
      </div>
    );
  }
};
