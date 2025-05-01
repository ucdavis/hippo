import { useEffect, useMemo, useState } from "react";
import { AccountModel, OrderListModel } from "../../types";
import { useParams } from "react-router-dom";
import { authenticatedFetch } from "../../util/api";

import HipTitle from "../../Shared/Layout/HipTitle";
import HipBody from "../../Shared/Layout/HipBody";
import HipMainWrapper from "../../Shared/Layout/HipMainWrapper";
import HipErrorBoundary from "../../Shared/LoadingAndErrors/HipErrorBoundary";
import HipClientError from "../../Shared/LoadingAndErrors/HipClientError";
import HipLoadingTable from "../../Shared/LoadingAndErrors/HipLoadingTable";
import { sortByDate } from "../../Shared/Table/HelperFunctions";
import { GroupNameWithTooltip } from "../Group/GroupNameWithTooltip";
import { getGroupModelString } from "../../util/StringHelpers";
import { createColumnHelper } from "@tanstack/react-table";
import { HipTable } from "../../Shared/Table/HipTable";

export const AccountDeactivations = () => {
  const [accounts, setAccounts] = useState<AccountModel[]>();
  const { cluster } = useParams(); //ExpiringOrders or ArchivedOrders or ProblemOrders


  useEffect(() => {
    const fetchAccounts = async () => {
      const response = await authenticatedFetch(
        `/api/${cluster}/report/accountdeactivations`,
      );

      if (response.ok) {
        const data = await response.json();
        setAccounts(data);
      } else {
        alert("Error fetching Accounts");
      }
    };

    fetchAccounts();
  }, [cluster]);

  const columnHelper = createColumnHelper<AccountModel>();

  const columns = [
    columnHelper.accessor((row) => getGroupModelString(row.memberOfGroups), {
      header: "Groups",
      cell: (props) => (
        <>
          {props.row.original.memberOfGroups.map((g, i) => (
            <>
              {i > 0 && ", "}
              <GroupNameWithTooltip
                group={g}
                id={props.row.original.id.toString()}
                key={i}
              />
            </>
          ))}
        </>
      ),
      meta: {
        exportFn: (account) =>
          account.memberOfGroups.map((g) => g.displayName)?.join(", "),
      },
    }),
    columnHelper.accessor("name", {
      header: "Name",
      id: "Name", // id required for only this column for some reason
    }),
    columnHelper.accessor("email", {
      header: "Email",
    }),
    columnHelper.accessor("kerberos", {
      header: "Kerberos",
    }),
    columnHelper.accessor("deactivatedOn", {
      header: "Deactivated On",
      id: "DeactivatedOn",
      cell: (info) => info.getValue() ? new Date(info.getValue()).toLocaleDateString() : "", // Display formatted date
      sortingFn: (rowA, rowB, columnId) => sortByDate(rowA, rowB, columnId),
    }),
    // Just using any property not already referenced in any other accessor column as a hacky 
    // way to have an accessor column for data that is not a direct property of the AccountModel.
    // TODO: Create a custom flat model for this whenever we do the same for ActiveAccounts.
    columnHelper.accessor("data", {
      header: "Group Revoked On",
      id: "GroupRevokedOn",
      cell: (info) => {
        const revokedOn = info.row.original.memberOfGroups.find(g => g.revokedOn)?.revokedOn;
        return revokedOn ? new Date(revokedOn).toLocaleDateString() : ""
      }, // Display formatted date
      sortingFn: (rowA, rowB) => {
        const dateA = rowA.original.memberOfGroups.find(g => g.revokedOn)?.revokedOn;
        const dateB = rowB.original.memberOfGroups.find(g => g.revokedOn)?.revokedOn;
        return (dateA ? new Date(dateA).getTime() : 0) - (dateB ? new Date(dateB).getTime() : 0);
      },
    }),
    columnHelper.accessor((row) => row.tags?.join(", "), {
      header: "Tags",
    }),
  ];

  const accountsData = useMemo(() => accounts ?? [], [accounts]);

  // RH TODO: handle loading/error states
  const Title = (
    <HipTitle
      title={"Account Deactivations"}
      subtitle={"Deactivations and Group Membership Revocations"}
    />
  );
  if (accounts === undefined) {
    return (
      <HipMainWrapper>
        {Title}
        <HipBody>
          <HipLoadingTable />
        </HipBody>
      </HipMainWrapper>
    );
  } else {
    return (
      <HipMainWrapper>
        {Title}
        <HipBody>
          <HipErrorBoundary
            fallback={
              <HipClientError
                type="alert"
                thereWasAnErrorLoadingThe="Orders Table"
                contactLink={true}
              />
            }
          >
            <HipTable
              columns={columns}
              data={accountsData}
              initialState={{
                sorting: [
                  { id: "GroupRevokedOn", desc: true },
                ],
              }}
            />
          </HipErrorBoundary>
        </HipBody>
      </HipMainWrapper>
    );
  }
};
