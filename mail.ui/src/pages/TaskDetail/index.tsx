import { mail_schedule_task_detail,mail_schedule_task_detail_delete } from '@/services/ant-design-pro/api';
import { FileExcelFilled,MinusOutlined,BackwardOutlined } from '@ant-design/icons';
import type { ActionType, ProColumns } from '@ant-design/pro-components';
import {
  ModalForm,
  PageContainer,
  ProTable
} from '@ant-design/pro-components';
import { Button, Drawer, message } from 'antd';
import React, { useRef, useState } from 'react';
import { parse } from 'querystring';

const TaskDetail: React.FC = () => {
  /**
   * @en-US Pop-up window of new window
   * @zh-CN 新建窗口的弹窗
   *  */
  const [createModalOpen, handleModalOpen] = useState<boolean>(false);

  const actionRef = useRef<ActionType>();
  const [selectedRowsState, setSelectedRows] = useState<API.ScheduleTaskDetail[]>([]);
  const [currentRow, setCurrentRow] = useState<API.ScheduleTaskDetail>();
  const [showDetail, setShowDetail] = useState<boolean>(false);

  const taskName = parse(window.location.href.split('?')[1]).id;

  const columns: ProColumns<API.ScheduleTaskDetail>[] = [
    {
      title: '目标邮件',
      dataIndex: 'Mail',
      tip: '计划时间到后向目标邮件发送邮件信息',
      render: (dom, entity) => {
        return (
          <a
            onClick={() => {
              console.log(entity);
              setCurrentRow(entity);
              setShowDetail(true);
            }}
          >
            {dom}
          </a>
        );
      },
    },
    {
      title: '计划发送时间',
      dataIndex: 'ScheduleTime',
      valueType: 'dateTime',
    },
    {
      title: '昵称',
      dataIndex: 'NickName',
      valueType: 'text',
    },
    {
      title: '计划类型',
      dataIndex: 'MailType',
      tip: '公告,特殊时间,如生日,法定假期',
      valueEnum: {
       1: {
        text: '公告'
       },
       2: {
        text: '特殊时间'
       }
      }
    },
    {
      title: '主题',
      dataIndex: 'Subject',
      valueType: 'text',
    },
    {
      title: '是否已发送',
      dataIndex: 'IsSend',
      renderText: (val: boolean) =>
        val ? '是' : '否'
    },
    {
      title: '发送时间',
      dataIndex: 'LastSend',
      valueType: 'dateTime',
    }
  ];
  return (
    <PageContainer>
      <ProTable<API.ScheduleTaskDetail, API.PageParams>
        headerTitle={'任务明细-' + taskName}
        actionRef={actionRef}
        rowKey='Index'
        search={false}
        toolBarRender={() => [
          <Button
            type="primary"
            key="primary"
            onClick={async () => {
              window.location.href = './tasklist'
            }}
          >
            <BackwardOutlined /> 返回
          </Button>,
          <Button
            type="primary"
            key="primary"
            onClick={async () => {
              var params = {
                'batch_id': taskName
              };
              const request = {
                body: JSON.stringify(params),
                method: 'POST',
                headers: {
                  'Content-Type': 'application/json;charset=UTF-8'
                }
              }
              const response = await fetch('/api/mail/downloadschedulemaillist', request)
              var name = `${taskName}.xlsx`;
              var url = window.URL.createObjectURL(await response.blob());
              var aLink = document.createElement("a");
              aLink.style.display = "none";
              aLink.href = url;
              aLink.setAttribute("download", name);
              document.body.appendChild(aLink);
              aLink.click();
              document.body.removeChild(aLink); //下载完成移除元素
              window.URL.revokeObjectURL(url); //释放掉blob对象放掉blob对象
            }}
          >
            <FileExcelFilled /> 下载任务
          </Button>,
          <Button
          type="primary"
          key="primary"
          onClick={() => {
            handleModalOpen(true);
          }}
        >
          <MinusOutlined /> 删除选中
        </Button>,
        ]}
        request={mail_schedule_task_detail}
        columns={columns}
        rowSelection={{
          onChange: (_, selectedRows) => {
            setSelectedRows(selectedRows);
          },
        }}
      />
      <ModalForm
        title='任务明细删除管理'
        width="400px"
        open={createModalOpen}
        onOpenChange={handleModalOpen}
        onFinish={async () => {
          var removeList = [];
          for(var i = 0; i < selectedRowsState.length; i++) {
            removeList.push(taskName + '-' + selectedRowsState[i].Index);
          }
          var post_data = {
            batch_ids: JSON.stringify(removeList)
          };
          var respObj = await mail_schedule_task_detail_delete(post_data);

          if(respObj.success) {
            message.success('任务删除成功!');
          }
          else {
            message.warning('任务删除失败!' + respObj.error);
          }
          actionRef.current?.reload();
          return true;
        }}
      >
        {'确定删除选中的' + selectedRowsState.length + '个明细任务？' }
      </ModalForm>
      <Drawer
        width={720}
        open={showDetail}
        onClose={() => {
          setTimeout(()=>{setCurrentRow(undefined)}, 500);
          setShowDetail(false);
        }}
        closable={false}
      >
        <meta name="referrer" content="no-referrer" />
        <p><h2 style={{"fontWeight":"bold"}}>{currentRow?.Subject}</h2></p>
        <div dangerouslySetInnerHTML={{__html:currentRow?.MailBody!}} ></div>
      </Drawer>
    </PageContainer>
  );
};

export default TaskDetail;
