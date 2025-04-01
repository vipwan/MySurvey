import React, { useState, useEffect } from 'react';
import { Form, Input, DatePicker, Button, Card, message, Space } from 'antd';
import { useNavigate, useParams } from 'react-router-dom';
import { surveyApi } from '../../services/api';
import dayjs from 'dayjs';

const SurveyEdit = () => {
    const [form] = Form.useForm();
    const [loading, setLoading] = useState(false);
    const navigate = useNavigate();
    const { id } = useParams();

    useEffect(() => {
        if (id) {
            fetchSurvey();
        }
    }, [id]);

    const fetchSurvey = async () => {
        try {
            setLoading(true);
            const response = await surveyApi.getSurvey(id);
            const survey = response.data;
            form.setFieldsValue({
                title: survey.title,
                description: survey.description,
                startTime: dayjs(survey.startTime),
                endTime: dayjs(survey.endTime),
            });
        } catch (error) {
            message.error('获取问卷信息失败');
        } finally {
            setLoading(false);
        }
    };

    const onFinish = async (values) => {
        try {
            setLoading(true);
            const data = {
                title: values.title,
                description: values.description,
                startTime: values.startTime.toISOString(),
                endTime: values.endTime.toISOString(),
            };

            if (id) {
                await surveyApi.updateSurvey(id, data);
                message.success('更新成功');
            } else {
                await surveyApi.createSurvey(data);
                message.success('创建成功');
            }

            navigate('/surveys');
        } catch (error) {
            message.error(id ? '更新失败' : '创建失败');
        } finally {
            setLoading(false);
        }
    };

    return (
        <Card title={id ? '编辑问卷' : '创建问卷'}>
            <Form
                form={form}
                layout="vertical"
                onFinish={onFinish}
                initialValues={{
                    startTime: dayjs(),
                    endTime: dayjs().add(7, 'day'),
                }}
            >
                <Form.Item
                    name="title"
                    label="问卷标题"
                    rules={[{ required: true, message: '请输入问卷标题' }]}
                >
                    <Input placeholder="请输入问卷标题" />
                </Form.Item>

                <Form.Item
                    name="description"
                    label="问卷描述"
                >
                    <Input.TextArea rows={4} placeholder="请输入问卷描述" />
                </Form.Item>

                <Form.Item
                    name="startTime"
                    label="开始时间"
                    rules={[{ required: true, message: '请选择开始时间' }]}
                >
                    <DatePicker
                        showTime
                        format="YYYY-MM-DD HH:mm:ss"
                        style={{ width: '100%' }}
                    />
                </Form.Item>

                <Form.Item
                    name="endTime"
                    label="结束时间"
                    rules={[
                        { required: true, message: '请选择结束时间' },
                        ({ getFieldValue }) => ({
                            validator(_, value) {
                                if (!value || value.isAfter(getFieldValue('startTime'))) {
                                    return Promise.resolve();
                                }
                                return Promise.reject(new Error('结束时间必须晚于开始时间'));
                            },
                        }),
                    ]}
                >
                    <DatePicker
                        showTime
                        format="YYYY-MM-DD HH:mm:ss"
                        style={{ width: '100%' }}
                    />
                </Form.Item>

                <Form.Item>
                    <Space>
                        <Button type="primary" htmlType="submit" loading={loading}>
                            {id ? '保存' : '创建'}
                        </Button>
                        <Button onClick={() => navigate('/surveys')}>
                            取消
                        </Button>
                    </Space>
                </Form.Item>
            </Form>
        </Card>
    );
};

export default SurveyEdit; 