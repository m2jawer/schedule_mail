import { mail_schedule_task_list, mail_schedule_task_delete } from '@/services/ant-design-pro/api';
import { MinusOutlined,PlusOutlined } from '@ant-design/icons';
import type { ActionType, ProColumns } from '@ant-design/pro-components';
import {
  ModalForm,
  PageContainer,
  ProTable,
} from '@ant-design/pro-components';
import { Button, Upload, message } from 'antd';
import React, { useRef, useState } from 'react';
import { UploadChangeParam } from 'antd/es/upload';
import { UploadFile } from 'antd/lib';

const TaskList: React.FC = () => {
  /**
   * @en-US Pop-up window of new window
   * @zh-CN 新建窗口的弹窗
   *  */
  const [createModalOpen, handleModalOpen] = useState<boolean>(false);

  const actionRef = useRef<ActionType>();
  const [selectedRowsState, setSelectedRows] = useState<API.ScheduleTask[]>([]);
  const [uploadList, setUploadList] = useState<any[]>([]);
  
  const handleUpload = async (info: UploadChangeParam<UploadFile>) => {
    if(info.file.status == 'uploading') {
      setUploadList([{...info.fileList[0]}]);
    }
    else if(info.file.status == 'done') {
      if(info.file.response.success) {
        actionRef.current!.reload();
        message.success('任务上传成功!');
      }
      else {
        message.warning('任务上传失败!');
      }
    }
    else {
      message.warning('任务上传失败!' + info.file.status);
    }
  };

  const columns: ProColumns<API.ScheduleTask>[] = [
    {
      title: '任务名称',
      dataIndex: 'Name',
      tip: '上传时候的文件名',
      render: (dom, entity) => {
        return (
          <a
            href={'/mail/taskdetail?id=' + entity.Name}
          >
            {entity.Name}
          </a>
        );
      },
    },
    {
      title: '子任务数量',
      dataIndex: 'TaskCount',
      valueType: 'text',
    },
    {
      title: '未发送',
      dataIndex: 'UnSendCount',
      valueType: 'text',
    },
    {
      title: '已发送',
      dataIndex: 'SendCount',
      valueType: 'text',
    },
    {
      title: '任务创建时间',
      dataIndex: 'CreateAt',
      valueType: 'dateTime',
    },
    {
      title: '任务更新时间',
      dataIndex: 'UpdateAt',
      valueType: 'dateTime',
    },
    {
      title: '任务是否已完成',
      dataIndex: 'IsFinish',
      renderText: (val: boolean) =>
        val ? '是' : '否'
    }
  ];

  return (
    <PageContainer>
      <ProTable<API.ScheduleTask, API.PageParams>
        headerTitle='任务列表查询'
        actionRef={actionRef}
        rowKey="Name"
        toolBarRender={() => [
          <Upload action={'/mail/api/mail/uploadschedule'} accept='.csv,.xls,.xlsx' fileList={uploadList} maxCount={1} onChange={handleUpload} showUploadList={false}>
            <Button
              type="primary"
              key="primary"
              onClick={() => {
                
              }}
            >
              <PlusOutlined /> 上传任务
            </Button>
          </Upload>,
          <Button
            type="primary"
            key="primary"
            onClick={() => {
              if(selectedRowsState.length == 0) {
                message.info('请先选中需要删除的任务!');
                return;
              }
              handleModalOpen(true);
            }}
          >
            <MinusOutlined /> 删除选中
          </Button>,
        ]}
        search={{
          labelWidth: 120,
        }}
        request={mail_schedule_task_list}
        columns={columns}
        rowSelection={{
          onChange: (_, selectedRows) => {
            setSelectedRows(selectedRows);
          },
        }}
      />
      <ModalForm
        title='任务删除管理'
        width="400px"
        open={createModalOpen}
        onOpenChange={handleModalOpen}
        onFinish={async () => {
          var removeList = [];
          for(var i = 0; i < selectedRowsState.length; i++) {
            removeList.push(selectedRowsState[i].Name);
          }
          var post_data = {
            batch_ids: JSON.stringify(removeList)
          };
          var respObj = await mail_schedule_task_delete(post_data);

          if(respObj.success) {
            message.success('任务删除成功!');
          }
          else {
            message.warning('任务删除失败!' + respObj.error);
          }
          actionRef.current?.reload();
          //handleModalOpen(false);
          return true;
        }}
      >
        {'确定删除选中的' + selectedRowsState.length + '个任务？' }
      </ModalForm>
    </PageContainer>
  );
};

export default TaskList;
