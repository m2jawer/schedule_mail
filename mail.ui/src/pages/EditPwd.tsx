import { PageContainer,ProForm,ProFormCheckbox,ProFormText } from '@ant-design/pro-components';
import { edit_pwd } from '@/services/ant-design-pro/api';
import {
  UserOutlined,
} from '@ant-design/icons';
import { FormattedMessage,useModel } from '@umijs/max';
import { message } from 'antd';
import React from 'react';

const EditPwd: React.FC = () => {
  const { initialState } = useModel('@@initialState');
  return (
    <div
        style={{
          flex: '1',
          padding: '32px 0',
        }}
    >
      <ProForm name="pwd_form"
        onFinish={async (paras)=>{
          paras.username = initialState?.currentUser?.access;
          var respObj = await edit_pwd(paras)
          if(respObj.success) {
            message.success('密码更新成功');
          }
          else {
            message.error('密码更新失败');
          }
          return true;
        }}
      >
      <div
        style={{
          minWidth: 180,
          maxWidth: '10vw',
        }}
      >
        <ProFormText.Password
          name="pwd_old"
          fieldProps={{
            size: 'large',
            prefix: <UserOutlined />,
          }}
          placeholder='旧密码'
          rules={[
            {
              required: true,
              message: (
                <FormattedMessage id="old.pwd.empty"
                  defaultMessage="旧密码不可以为空!"
                />
              ),
            },
          ]}
        />
      </div>
      <div
        style={{
          minWidth: 180,
          maxWidth: '10vw',
        }}
      >
        <ProFormText.Password
          name="pwd"
          fieldProps={{
            size: 'large',
            prefix: <UserOutlined />,
          }}
          placeholder='新密码'
          rules={[
            {
              required: true,
              message: (
                <FormattedMessage id="new.pwd.empty"
                  defaultMessage="密码不可以设置为空!"
                />
              ),
            },
          ]}
        />
      </div>
      <div
        style={{
          minWidth: 180,
          maxWidth: '10vw',
        }}
      >
        <ProFormText.Password
          name="pwd2"
          fieldProps={{
            size: 'large',
            prefix: <UserOutlined />,
          }}
          placeholder='确认新密码'
          rules={[
            {
              required: true,
              message: (
                <FormattedMessage id="old.pwd2.empty"
                  defaultMessage="确认密码不可以为空!"
                />
              ),
            },
          ]}
        />
      </div>
      </ProForm>
    </div>
  );
};

export default EditPwd;
