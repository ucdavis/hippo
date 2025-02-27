import React, { useEffect, useState } from "react";
import ObjectTree from "../../Shared/ObjectTree";
import { AccountModel, GroupMembersModel } from "../../types";
import { useParams } from "react-router-dom";
import NotFound from "../../NotFound";
import { createColumnHelper } from "@tanstack/react-table";
import HipButton from "../../Shared/HipComponents/HipButton";
import HipBody from "../../Shared/Layout/HipBody";
import HipMainWrapper from "../../Shared/Layout/HipMainWrapper";
import HipTitle from "../../Shared/Layout/HipTitle";
import HipLoading from "../../Shared/LoadingAndErrors/HipLoading";
import { HipTable } from "../../Shared/Table/HipTable";
import { authenticatedFetch, parseBadRequest } from "../../util/api";
import { useConfirmationDialog } from "../../Shared/ConfirmationDialog";
import { GroupInfo } from "./GroupInfo";
import { usePromiseNotification } from "../../util/Notifications";

const GroupMembers: React.FC = () => {
  const { cluster: clusterName, groupId: groupIdStr } = useParams<{
    cluster: string;
    groupId: string;
  }>();
  const groupId = parseInt(groupIdStr);
  const [removing, setRemoving] = useState<AccountModel>();
  const [groupMembers, setGroupMembers] = useState<GroupMembersModel>();
  const [_, setNotification] = usePromiseNotification();

  useEffect(() => {
    const fetchAccounts = async () => {
      const response = await authenticatedFetch(
        `/api/${clusterName}/account/groupMembers?groupId=${groupId}`,
      );

      if (response.ok) {
        setGroupMembers((await response.json()) as GroupMembersModel);
      }
    };

    fetchAccounts();
  }, [clusterName, groupId]);

  const details = {
    name: groupMembers?.group.name,
    displayName: groupMembers?.group.displayName,
    admins: (groupMembers?.group.admins ?? []).map((ga) => ({
      name: ga.name,
      email: ga.email,
    })),
    ...(groupMembers?.group.data ?? {}),
  };

  const [showDetails] = useConfirmationDialog(
    {
      title: "Group Details",
      message: () => {
        return <ObjectTree obj={details} />;
      },
      buttons: ["OK"],
    },
    [details],
  );

  const handleViewDetails = async () => {
    await showDetails();
  };

  const [confirmRemove] = useConfirmationDialog(
    {
      title: `Remove account from group`,
      message: `You are about to request removal of account ${removing?.name} from group ${groupMembers?.group.name}`,
    },
    [removing, groupMembers],
  );

  const handleRemove = async (account: AccountModel) => {
    setRemoving(account);
    const [confirmed] = await confirmRemove();
    if (confirmed) {
      const request = authenticatedFetch(
        `/api/${clusterName}/group/RequestRemoveMember`,
        {
          method: "POST",
          body: JSON.stringify({ groupId, accountId: account.id }),
        },
      );
      setNotification(
        request,
        "Requesting removal of group member",
        "Removal of group member requested",
        async (r) => {
          if (r.status === 400) {
            const errors = await parseBadRequest(response);
            return errors;
          } else {
            return "An error happened, please try again.";
          }
        },
      );

      const response = await request;
      if (response.ok) {
        setGroupMembers((groupMembersModel) => ({
          ...groupMembersModel,
          kerberosPendingRemoval: [
            ...groupMembersModel.kerberosPendingRemoval,
            account.kerberos,
          ],
        }));
      }
    }
    setRemoving(undefined);
  };

  if (groupMembers && !groupMembers.group) {
    return <NotFound />;
  }

  const columnHelper = createColumnHelper<AccountModel>();

  const columns = [
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
    columnHelper.accessor(
      (row) => new Date(row.updatedOn).toLocaleDateString(),
      {
        header: "Updated On",
      },
    ),
    columnHelper.accessor((row) => row.tags?.join(", "), {
      header: "Tags",
    }),
    columnHelper.display({
      id: "actions",
      header: "Action",
      cell: (props) => {
        const removalPending = groupMembers.kerberosPendingRemoval.some(
          (kerb) => props.row.original.kerberos === kerb,
        );
        return (
          <>
            <HipButton
              disabled={removalPending}
              onClick={() => handleRemove(props.row.original)}
            >
              {removalPending
                ? "Removal Pending"
                : removing?.id === props.row.original.id
                  ? "Requesting Removal..."
                  : "Remove from Group"}
            </HipButton>
          </>
        );
      },
    }),
  ];

  const Title = (
    <HipTitle title="Group Member Accounts" subtitle="Administration" />
  );
  if (groupMembers === undefined) {
    return (
      <HipMainWrapper>
        {Title}
        <HipBody>
          <HipLoading />
        </HipBody>
      </HipMainWrapper>
    );
  }

  return (
    <HipMainWrapper>
      {Title}
      <HipBody>
        <p>
          <GroupInfo
            group={groupMembers.group}
            showDetails={handleViewDetails}
          />
        </p>
        <HipTable
          columns={columns}
          data={groupMembers.accounts}
          initialState={{
            sorting: [
              { id: "Groups", desc: false },
              { id: "Name", desc: false },
            ],
          }}
        />
      </HipBody>
    </HipMainWrapper>
  );
};

export default GroupMembers;
